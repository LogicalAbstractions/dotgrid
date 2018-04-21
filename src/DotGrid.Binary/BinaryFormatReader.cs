using System.Text;
using System.Text.Utf8;

namespace DotGrid.Binary
{
    public sealed class BinaryFormatReader
    {
        private readonly BinaryFormatReadingContext context;
        private readonly BinaryFormatMemoryReader reader;
        private readonly int propertyOffsetsPosition;
        private readonly int propertyCount;
        private readonly int rootMetadataPosition;
        private readonly BinaryFormatValueType rootContainerType;

        private int currentMetadataPosition;
        private BinaryFormatValueType currentContainerType;
        private int currentDepth;
        private int currentElementCount;
        private (int, int,int, BinaryFormatValueType, int) lastPropertyDescription;

        public struct ContainerHandle
        {
            internal int Position;
            internal BinaryFormatValueType ContainerType;
            internal int Depth;

            internal ContainerHandle(int position, BinaryFormatValueType containerType, int depth)
            {
                Position = position;
                ContainerType = containerType;
                Depth = depth;
            }
        }

        public BinaryFormatReader(BinaryFormatReadingContext context,BinaryFormatMemoryReader reader)
        {
            this.context = context;
            this.reader = reader;
            
            this.context.Reset();
            
            // Find the initial metadata positions:
            reader.Seek(reader.Size - 16);

            rootMetadataPosition = reader.Position - reader.ReadInt();
            propertyOffsetsPosition = reader.Position - reader.ReadInt();
            rootContainerType = (BinaryFormatValueType) reader.ReadInt();
            propertyCount = reader.ReadInt();
            
            Top();
        }

        public ContainerHandle CurrentContainer => new ContainerHandle(currentMetadataPosition, 
            currentContainerType, 
            currentDepth);

        public BinaryFormatValueType ContainerType => currentContainerType;

        public int ElementCount => currentElementCount;

        public int Depth => currentDepth;

        internal BinaryFormatReadingContext Context => context;

        public int GetPropertyId(int index)
        {
            if (currentContainerType == BinaryFormatValueType.Array)
            {
                return index;
            }
            
            var propertyAddress = 8 + index * 12;
            
            reader.Seek(currentMetadataPosition + propertyAddress);

            return reader.ReadInt();
        }

        public bool TryGetPropertyId(Utf8String name, out int id)
        {
            if (!context.TryGetPropertyId(name, out id))
            {
                return TryFindPropertyId(name, out id);
            }

            return true;
        }

        public bool TryGetPropertyName(int id, out Utf8String name)
        {
            if (!context.TryGetPropertyName(id, out name))
            {
                if (TryFindPropertyName(id, out var nameSpan))
                {
                    name = new Utf8String(nameSpan);
                    return true;
                }

                return false;
            }

            return true;
        }
        
        public BinaryFormatValueType GetValueType(int idOrIndex)
        {
            if (TryFindProperty(idOrIndex, out var propertyDescription))
            {
                return propertyDescription.Item3;
            }
            
            ThrowInvalidProperty(idOrIndex);
            return BinaryFormatValueType.Undefined;
        }

        public bool GetBooleanValue(int idOrIndex)
        {
            SeekToValue(idOrIndex);

            return reader.ReadBoolean();
        }
                
        public byte GetByteValue(int idOrIndex)
        {
            SeekToValue(idOrIndex);

            return reader.ReadByte();
        }       
        
        public short GetShortValue(int idOrIndex)
        {
            SeekToValue(idOrIndex);

            return reader.ReadShort();
        }
             
        public int GetIntValue(int idOrIndex)
        {
            SeekToValue(idOrIndex);

            return reader.ReadInt();
        }
              
        public long GetLongValue(int idOrIndex)
        {
            SeekToValue(idOrIndex);

            return reader.ReadLong();
        }
               
        public float GetFloatValue(int idOrIndex)
        {
            SeekToValue(idOrIndex);

            return reader.ReadFloat();
        }       
        
        public double GetDoubleValue(int idOrIndex)
        {
            SeekToValue(idOrIndex);

            return reader.ReadDouble();
        }    
        
        public Utf8Span GetStringValue(int idOrIndex)
        {
            SeekToValue(idOrIndex);

            return ReadStringInternal();
        }

        public int GetBlobSize(int idOrIndex)
        {
            SeekToValue(idOrIndex);

            return reader.ReadInt();
        }

        public unsafe int GetBlobValue(int idOrIndex, byte[] target, int offset, int count)
        {
            fixed (byte* targetPtr = &target[0])
            {
                return GetBlobValue(idOrIndex,(byte*) (targetPtr + offset),  count);
            }
        }

        public unsafe int GetBlobValue(int idOrIndex,byte* targetPtr, int count)
        {
            SeekToValue(idOrIndex);

            int size = reader.ReadInt();

            int readCount = count < size ? count : size;
            
            reader.ReadBytes(targetPtr,readCount);

            return readCount;
        }
     
        public void Down(int idOrIndex)
        {
            if (TryFindProperty(idOrIndex, out var propertyDescription))
            {
                context.PushContainer(propertyDescription.Item3,propertyDescription.Item2);
                
                SeekToContainerInternal(propertyDescription.Item2,propertyDescription.Item3,currentDepth +1);
            }
            else
            {
                ThrowInvalidProperty(idOrIndex);
            }
        }

        public void Up(int steps = 1)
        {
            for (var i = 0; i < steps; ++i)
            {
                context.TryPopContainer(out var newEntry);

                if (i + 1 == steps)
                {
                    SeekToContainerInternal(newEntry.Position,newEntry.ContainerType,currentDepth - steps);
                }
            }
        }

        public void Top()
        {
            context.ClearContainers();
            SeekToContainerInternal(rootMetadataPosition,rootContainerType,0);
            
            context.PushContainer(rootContainerType,rootMetadataPosition);
        }

        public void Goto(ContainerHandle containerHandle)
        {
            SeekToContainerInternal(containerHandle.Position,containerHandle.ContainerType,containerHandle.Depth);
        }

        private void SeekToContainerInternal(int position,BinaryFormatValueType containerType,int depth)
        {
            currentContainerType = containerType;
            currentDepth = depth;
            currentMetadataPosition = position;
            
            reader.Seek(position);

            currentElementCount = reader.ReadInt();
        }

        private void SeekToValue(int idOrIndex)
        {
            if (TryFindProperty(idOrIndex, out var propertyDescription))
            {
                reader.Seek(propertyDescription.Item2);
            }
            else
            {
                ThrowInvalidProperty(idOrIndex);
            }
        }

        private bool TryFindProperty(int idOrIndex, out (int,int,BinaryFormatValueType,int) propertyDescription)
        {
            reader.Seek(currentMetadataPosition);

            var entryCount = reader.ReadInt();
            
            if (currentContainerType == BinaryFormatValueType.Array)
            {
                if (idOrIndex < entryCount)
                {
                    // We can directly calculate the address:
                    var startAddress = idOrIndex * 8;
                    
                    reader.Seek(currentMetadataPosition + 4 + startAddress);

                    var offset = reader.ReadInt();
                    var type = (BinaryFormatValueType) reader.ReadInt();

                    propertyDescription = (idOrIndex, currentMetadataPosition - offset, type,0);
                    return true;
                }
            }

            if (currentContainerType == BinaryFormatValueType.Object)
            {
                if (lastPropertyDescription.Item1 == currentMetadataPosition && lastPropertyDescription.Item2 == idOrIndex)
                {
                    propertyDescription = (lastPropertyDescription.Item2, lastPropertyDescription.Item3,
                        lastPropertyDescription.Item4, lastPropertyDescription.Item5);
                    return true;
                }
                
                var minIndex = 0;
                var maxIndex = currentElementCount - 1;
                var found = false;

                while (!found)
                {
                    var guessIndex = (maxIndex + minIndex) / 2;
                    
                    var propertyPosition = currentMetadataPosition + 8 + guessIndex * 12;
                    reader.Seek(propertyPosition);
            
                    var candidateId = reader.ReadInt();

                    if (candidateId != idOrIndex)
                    {
                        minIndex = candidateId < idOrIndex ? guessIndex + 1 : minIndex;
                        maxIndex = candidateId > idOrIndex ? guessIndex - 1 : maxIndex;
                    }
                    else
                    {
                        reader.Seek(propertyPosition - 4);
                        
                        var offset = reader.ReadInt();
                        
                        reader.Seek(propertyPosition + 4);
                        
                        var type = (BinaryFormatValueType) reader.ReadInt();

                        propertyDescription = (idOrIndex, currentMetadataPosition - offset, type,0);
                        lastPropertyDescription = (currentMetadataPosition, idOrIndex, currentMetadataPosition - offset,
                            type, 0);
                        return true;
                    }
                }
            }
                
            propertyDescription = (0, 0, BinaryFormatValueType.Undefined,0);
            return false;
        }

        private bool TryFindPropertyName(int id, out Utf8Span name)
        {
            if (id < propertyCount)
            {
                var propertyOffset = id * 4;

                reader.Seek(propertyOffsetsPosition + propertyOffset);

                var namePosition = propertyOffsetsPosition - reader.ReadInt();

                reader.Seek(namePosition);
                
                name = ReadStringInternal();
                context.SetPropertyName(id,new Utf8String(name));
                return true;
            }
            
            return false;
        }

        private bool TryFindPropertyId(Utf8String name,out int id)
        {
            // TODO: Use binary search
            for (var i = 0; i < propertyCount; ++i)
            {
                reader.Seek(propertyOffsetsPosition + i * 4);
                
                var propertyNameOffset = reader.ReadInt();
                var propertyNamePosition = propertyOffsetsPosition - propertyNameOffset;
                
                reader.Seek(propertyNamePosition);
                
                var propertyName = ReadStringInternal();
                
                context.SetPropertyId(new Utf8String(propertyName), i);

                if (propertyName.Equals(name))
                {
                    id = i;
                    return true;
                }
            }

            id = 0;
            return false;
        }
        
        private Utf8Span ReadStringInternal()
        {
            var byteCount = reader.ReadInt();

            return new Utf8Span(reader.ReadBytes(byteCount));
        }

        private void ThrowInvalidProperty(int idOrIndex)
        {
            if (currentContainerType == BinaryFormatValueType.Array)
            {
                throw new BinaryFormatException($"");
            }
            
            throw new BinaryFormatException($"Cannot resolve property for ");
        }
    }
}