using System;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using System.Text.Utf8;

namespace DotGrid.Binary2
{
    internal readonly struct ContainerAccessor
    {
        private readonly MemoryReader reader;
        private readonly int metadataPosition;
        private readonly ValueType containerType;

        internal readonly struct ValueEntry
        {
            internal readonly int Position;
            internal readonly ValueType ValueType;

            internal ValueEntry(int position, ValueType valueType)
            {
                Position = position;
                ValueType = valueType;
            }
        }

        internal ValueType ContainerType => containerType;

        internal int EntryCount
        {
            get
            {
                reader.Seek(metadataPosition + 2);
                return reader.ReadInt();
            }
        }

        internal ContainerAccessor(MemoryReader reader, int metadataPosition, ValueType containerType)
        {
            this.reader = reader;
            this.metadataPosition = metadataPosition;
            this.containerType = containerType;
        }

        internal bool ReadBoolean(int idOrIndex)
        {
            MoveToValueEntry(idOrIndex);

            return reader.ReadBoolean();
        }

        internal byte ReadByte(int idOrIndex)
        {
            MoveToValueEntry(idOrIndex);

            return reader.ReadByte();
        }

        internal short ReadShort(int idOrIndex)
        {
            MoveToValueEntry(idOrIndex);

            return reader.ReadShort();
        }

        internal int ReadInt(int idOrIndex)
        {
            MoveToValueEntry(idOrIndex);

            return reader.ReadSignedVarInt();
        }

        internal long ReadLong(int idOrIndex)
        {
            MoveToValueEntry(idOrIndex);
            
            return reader.ReadSignedVarLong();
        }

        internal float ReadFloat(int idOrIndex)
        {
            MoveToValueEntry(idOrIndex);

            return reader.ReadFloat();
        }

        internal double ReadDouble(int idOrIndex)
        {
            MoveToValueEntry(idOrIndex);

            return reader.ReadDouble();
        }

        internal Utf8Span ReadString(int idOrIndex)
        {
            MoveToValueEntry(idOrIndex);

            return reader.ReadUtf8Span();
        }

        internal ReadOnlySpan<byte> ReadBlob(int idOrIndex)
        {
            MoveToValueEntry(idOrIndex);

            var size = reader.ReadUnsignedVarInt();
            return reader.ReadBytes((int) size);
        }

        internal ContainerAccessor ReadContainer(int idOrIndex)
        {
            reader.Seek(metadataPosition);

            var entry = FindMetadataEntry(idOrIndex);
            
            return new ContainerAccessor(reader,entry.Position,ValueType.Object);
        }

        private void MoveToValueEntry(int idOrIndex)
        {
            reader.Seek(metadataPosition);

            var valueEntry = FindMetadataEntry(idOrIndex);
            
            reader.Seek(valueEntry.Position);
        }
        
        private ValueEntry FindMetadataEntry(int idOrIndex)
        {
            return containerType == ValueType.Array
                ? FindArrayMetadataEntry(idOrIndex)
                : FindObjectMetadataEntry(idOrIndex);
        }
        
        private ValueEntry FindArrayMetadataEntry(int index)
        {
            var offsetType = (FixedIntegerType)reader.ReadByte();

            var relativeMetadataPosition =
                1 + // Offset type
                4 + // Element count
                index *
                ((int) offsetType + 1);

            reader.Seek(metadataPosition + relativeMetadataPosition);
            
            return new ValueEntry(
                reader.ReadFixedInteger(offsetType) + metadataPosition,
                (ValueType)reader.ReadByte()
            );
        }

        private ValueEntry FindObjectMetadataEntry(int id)
        { 
            var offsetType = (FixedIntegerType) reader.ReadByte();
            var propertyIdType = (FixedIntegerType) reader.ReadByte();

            var entryCount = reader.ReadInt();
            
            var minIndex = 0;
            var maxIndex = entryCount - 1;
         
            while (minIndex < maxIndex)
            {
                var guessIndex = (maxIndex + minIndex) / 2;

                var propertyPosition = FindObjectMetadataEntryPositionByIndex(offsetType, propertyIdType, guessIndex);
                reader.Seek(propertyPosition  + (int)offsetType);

                var candidateId = reader.ReadFixedInteger(propertyIdType);

                if (candidateId == id)
                {
                    reader.Seek(propertyPosition);

                    var position = reader.ReadFixedInteger(offsetType) + metadataPosition;
                    reader.ReadFixedInteger(propertyIdType);

                    var valueType = (ValueType) reader.ReadByte();
                    
                    return new ValueEntry(position,valueType);
                }

                if (candidateId < id)
                {
                    minIndex = guessIndex + 1;
                }
                else if (candidateId > id)
                {
                    maxIndex = guessIndex - 1;
                }
            }

            return new ValueEntry(-1,ValueType.Undefined);
        }
        
        private int FindObjectMetadataEntryPositionByIndex(FixedIntegerType offsetType,FixedIntegerType propertyIdType,int index)
        {
            var relativeMetadataPosition =
                1 + // Offset type
                1 + // PropertyId type
                4 + // Element count
                index *
                ((int) offsetType + (int) propertyIdType + 1);

            return metadataPosition + relativeMetadataPosition;
        }
    }
}