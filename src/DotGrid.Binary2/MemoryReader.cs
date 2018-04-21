using System;
using System.Runtime.CompilerServices;
using System.Text.Utf8;

namespace DotGrid.Binary2
{
    public sealed unsafe class MemoryReader 
    {
        private readonly byte* sourcePtrOriginal;
        private readonly int size;
        
        private int position;
        private byte* currentPtr;
    
        public MemoryReader(byte* sourcePtr, int size)
        {
            this.sourcePtrOriginal = sourcePtr;
            this.size = size;

            this.position = 0;
            this.currentPtr = sourcePtrOriginal + this.position;
        }

        public int Position => (int)(position);

        public int Size => (int)size;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Seek(int position)
        {
            this.position = position;
            this.currentPtr = sourcePtrOriginal + this.position;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte ReadByte()
        {
            var result = currentPtr[0];

            this.position += 1;
            this.currentPtr += 1;

            return result;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short ReadShort()
        {
            var result = ((short*) currentPtr)[0];

            this.position += 2;
            this.currentPtr += 2;

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ReadInt()
        {
            var result = ((int*) currentPtr)[0];

            this.position += 4;
            this.currentPtr += 4;

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long ReadLong()
        {
            var result = ((long*) currentPtr)[0];

            this.position += 8;
            this.currentPtr += 8;

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float ReadFloat()
        {
            var result = ((float*) currentPtr)[0];

            this.position += 4;
            this.currentPtr += 4;

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double ReadDouble()
        {
            var result = ((double*) currentPtr)[0];

            this.position += 8;
            this.currentPtr += 8;

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ReadBoolean()
        {
            return ReadByte() == 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<byte> ReadBytes(int count)
        {
            var readSpan = new Span<byte>(currentPtr,count);
            this.position += count;
            this.currentPtr += count;
            return readSpan;
        }
    }

    public static class MemoryReaderExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Utf8Span ReadUtf8Span(this MemoryReader reader)
        {
            var byteCount = reader.ReadUnsignedVarInt();
            return new Utf8Span(reader.ReadBytes((int)byteCount));
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ReadSignedVarInt(this MemoryReader reader)
        {
            return DecodeZigZag32(ReadUnsignedVarInt(reader));
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ReadUnsignedVarInt(this MemoryReader reader)
        {
            int result = 0;
            int offset = 0;
            for (; offset < 32; offset += 7)
            {
                int b = reader.ReadByte();
                result |= (b & 0x7f) << offset;
                if ((b & 0x80) == 0)
                {
                    return (uint) result;
                }
            }
            
            // Keep reading up to 64 bits.
            for (; offset < 64; offset += 7)
            {
                int b = reader.ReadByte();
               
                if ((b & 0x80) == 0)
                {
                    return (uint) result;
                }
            }

            throw new FormatException("Invalid varint32 format");
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long ReadSignedVarLong(this MemoryReader reader)
        {
            return DecodeZigZag64(ReadUnsignedVarLong(reader));
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong ReadUnsignedVarLong(this MemoryReader reader)
        {
            int shift = 0;
            ulong result = 0;
            while (shift < 64)
            {
                byte b = reader.ReadByte();
                result |= (ulong) (b & 0x7F) << shift;
                if ((b & 0x80) == 0)
                {
                    return result;
                }
                shift += 7;
            }
            
            throw new FormatException("Invalid varint64 format");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int DecodeZigZag32(uint n)
        {
            return (int)(n >> 1) ^ -(int)(n & 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static long DecodeZigZag64(ulong n)
        {
            return (long)(n >> 1) ^ -(long)(n & 1);
        }
    }
}