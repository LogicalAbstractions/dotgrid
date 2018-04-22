using System;
using System.Text.Utf8;

namespace DotGrid.Binary2
{
    public class BinaryFormatReader
    {
        private MemoryReader reader;
        private int rootPosition;
        
        public void Open(MemoryReader reader)
        {
            this.reader = reader;
            
            // Find root position:
        }
        
        public bool ReadBoolean()
        {
            reader.Seek(rootPosition);
            return reader.ReadBoolean();
        }

        public byte ReadByte()
        {
            reader.Seek(rootPosition);
            return reader.ReadByte();
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
    }
}