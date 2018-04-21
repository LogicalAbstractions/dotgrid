using System;
using System.Runtime.CompilerServices;

namespace DotGrid.Binary
{
    public sealed unsafe class BinaryFormatMemoryReader 
    {
        private readonly byte* sourcePtrOriginal;
        private readonly int offset;
        private readonly int size;
        
        private int position;
        private byte* currentPtr;
    
        public BinaryFormatMemoryReader(byte* sourcePtr, int offset, int size)
        {
            this.sourcePtrOriginal = sourcePtr;
            this.offset = offset;
            this.size = size;

            this.position = offset;
            this.currentPtr = sourcePtrOriginal + this.position;
        }

        public int Position => (int)(position - offset);

        public int Size => (int)size;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Seek(int position)
        {
            SetLogicalPosition(position);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte ReadByte()
        {
            var result = currentPtr[0];

            IncrementPosition(1);
            
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReadBytes(byte* targetPtr, int count)
        {
            for (var i = 0; i < count; ++i)
            {
                targetPtr[i] = currentPtr[i];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short ReadShort()
        {
            var result = ((short*) currentPtr)[0];

            IncrementPosition(2);
            
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ReadInt()
        {
            var result = ((int*) currentPtr)[0];

            IncrementPosition(4);
            
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long ReadLong()
        {
            var result = ((long*) currentPtr)[0];
            
            IncrementPosition(8);

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float ReadFloat()
        {
            var result = ((float*) currentPtr)[0];
            
            IncrementPosition(4);

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double ReadDouble()
        {
            var result = ((double*) currentPtr)[0];
            
            IncrementPosition(8);

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
            IncrementPosition(count);
            return readSpan;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void IncrementPosition(int count)
        {
            this.position += count;
            this.currentPtr += count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetLogicalPosition(int logicalPosition)
        {
            this.position = logicalPosition + offset;
            this.currentPtr = sourcePtrOriginal + this.position;
        }
    }
}