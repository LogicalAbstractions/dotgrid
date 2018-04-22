using System;
using System.Text.Utf8;

namespace DotGrid.Binary2
{
    public struct BinaryFormatObjectReader
    {
        private ContainerAccessor accessor;
        private PropertyNamesReader namesReader;

        internal BinaryFormatObjectReader(ContainerAccessor accessor, PropertyNamesReader namesReader)
        {
            this.accessor = accessor;
            this.namesReader = namesReader;
        }

        public struct PropertyIdEnumerator
        {
            private ContainerAccessor accessor;
            private int index;

            internal PropertyIdEnumerator(ContainerAccessor accessor)
            {
                this.accessor = accessor;
                this.index = 0;
            }

            public int Current => accessor.GetPropertyIdByIndex(accessor.OffsetType, accessor.PropertyIdType, index);

            public void Reset()
            {
                index = 0;
            }

            public bool MoveNext()
            {
                index++;
                
                return index < accessor.EntryCount;
            }
        }

        public bool ReadBoolean(int propertyId)
        {
            return accessor.ReadBoolean(propertyId);
        }

        public byte ReadByte(int propertyId)
        {
            return accessor.ReadByte(propertyId);
        }

        public short ReadShort(int propertyId)
        {
            return accessor.ReadShort(propertyId);
        }

        public int ReadInt(int propertyId)
        {
            return accessor.ReadInt(propertyId);
        }

        public long ReadLong(int propertyId)
        {
            return accessor.ReadLong(propertyId);
        }

        public float ReadFloat(int propertyId)
        {
            return accessor.ReadFloat(propertyId);
        }

        public double ReadDouble(int propertyId)
        {
            return accessor.ReadDouble(propertyId);
        }

        public Utf8Span ReadString(int propertyId)
        {
            return accessor.ReadString(propertyId);
        }

        public ReadOnlySpan<byte> ReadBlob(int propertyId)
        {
            return accessor.ReadBlob(propertyId);
        }

        public BinaryFormatObjectReader ReadObject(int propertyId)
        {
            var objectAccessor = accessor.ReadContainer(propertyId);
            
            return new BinaryFormatObjectReader(objectAccessor,namesReader);
        }

        public BinaryFormatArrayReader ReadArray(int propertyId)
        {
            var arrayAccessor = accessor.ReadContainer(propertyId);
            
            return new BinaryFormatArrayReader(arrayAccessor,namesReader);
        }

        public ValueType GetValueType(int propertyId)
        {
            return accessor.GetValueType(propertyId);
        }
        
        public string GetPropertyName(int propertyId)
        {
            return namesReader.GetPropertyNameForId(propertyId);
        }

        public int GetPropertyId(string name)
        {
            return namesReader.GetPropertyIdForName(name);
        }

        public PropertyIdEnumerator GetEnumerator()
        {
            return new PropertyIdEnumerator(accessor);
        }
    }
}