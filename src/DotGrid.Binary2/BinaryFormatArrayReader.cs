using System;
using System.Text.Utf8;

namespace DotGrid.Binary2
{
    public struct BinaryFormatArrayReader
    {
        private ContainerAccessor accessor;
        private PropertyNamesReader namesReader;

        internal BinaryFormatArrayReader(ContainerAccessor accessor, PropertyNamesReader namesReader)
        {
            this.accessor = accessor;
            this.namesReader = namesReader;
        }
        
        public struct IndexEnumerator
        {
            private int index;
            private int elementCount;

            internal IndexEnumerator(int elementCount)
            {
                this.elementCount = elementCount;
                this.index = 0;
            }

            public int Current => index;

            public void Reset()
            {
                index = 0;
            }

            public bool MoveNext()
            {
                index++;

                return index < elementCount;
            }
        }

        public int Count => accessor.EntryCount;

        public bool ReadBoolean(int index)
        {
            return accessor.ReadBoolean(index);
        }

        public byte ReadByte(int index)
        {
            return accessor.ReadByte(index);
        }

        public short ReadShort(int index)
        {
            return accessor.ReadShort(index);
        }

        public int ReadInt(int index)
        {
            return accessor.ReadInt(index);
        }

        public long ReadLong(int index)
        {
            return accessor.ReadLong(index);
        }

        public float ReadFloat(int index)
        {
            return accessor.ReadFloat(index);
        }

        public double ReadDouble(int index)
        {
            return accessor.ReadDouble(index);
        }

        public Utf8Span ReadString(int index)
        {
            return accessor.ReadString(index);
        }

        public ReadOnlySpan<byte> ReadBlob(int index)
        {
            return accessor.ReadBlob(index);
        }

        public BinaryFormatObjectReader ReadObject(int index)
        {
            var objectAccessor = accessor.ReadContainer(index);
            
            return new BinaryFormatObjectReader(objectAccessor,namesReader);
        }

        public BinaryFormatArrayReader ReadArray(int index)
        {
            var arrayAccessor = accessor.ReadContainer(index);
            
            return new BinaryFormatArrayReader(arrayAccessor,namesReader);
        }
        
        public ValueType GetValueType(int index)
        {
            return accessor.GetValueType(index);
        }
        
        public IndexEnumerator GetEnumerator()
        {
            return new IndexEnumerator(accessor.EntryCount);
        }
    }
}