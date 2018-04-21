using System;
using System.Runtime.CompilerServices;

namespace DotGrid.Binary
{
    public sealed unsafe class BinaryFormatMemoryWriter 
    {
        private readonly byte* targetPtr;
        private readonly int offset;
        private readonly int size;
        
        private int position;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BinaryFormatMemoryWriter(byte* targetPtr, int offset, int size)
        {
            
            this.targetPtr = targetPtr;
            this.offset = offset;
            this.size = size;
            this.position = 0;
        }

        public int Position => position;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Seek(int position)
        {
            this.position = position;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(byte value)
        {
            *(targetPtr + GetIndex(1)) = value;
            Seek(position + 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(in ReadOnlySpan<byte> buffer)
        {
            var index = GetIndex(buffer.Length);
            var spanSize = size - index;
            var targetSpan = new Span<byte>(targetPtr + GetIndex(buffer.Length),spanSize);
            
            buffer.CopyTo(targetSpan);
            Seek(position + buffer.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(byte* sourcePtr, int offset, int count)
        {
#if CHECKED
            if (sourcePtr == null)
            {
                throw new ArgumentNullException(nameof(sourcePtr));
            }

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }
            
#endif
            
            var startIndex = GetIndex(count);
            var targetBasePtr = targetPtr + startIndex;
            var sourceBasePtr = sourcePtr + offset;
            
            for (var i = 0; i < count; ++i)
            {
                *targetBasePtr = *sourceBasePtr;
                targetBasePtr++;
                sourceBasePtr++;
                
                //*(targetPtr + startIndex + i) = *(sourcePtr + offset + i);
            }
            
            Seek(position + count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(short value)
        {
            var ptr = (short*) (targetPtr + GetIndex(2));
            *ptr = value;
            
            Seek(position + 2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(int value)
        {
            var ptr = (int*) (targetPtr + GetIndex(4));
            *ptr = value;
            
            Seek(position + 4);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(long value)
        {
            var ptr = (long*) (targetPtr + GetIndex(8));
            *ptr = value;
            
            Seek(position + 8);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(float value)
        {
            var ptr = (float*) (targetPtr + GetIndex(4));
            *ptr = value;
            
            Seek(position + 4);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(double value)
        {
            var ptr = (double*) (targetPtr + GetIndex(8));
            *ptr = value;
            
            Seek(position + 8);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(bool value)
        {
            var ptr = targetPtr + GetIndex(1);
            *ptr = (byte) (value ? 1 : 0);
            
            Seek(position + 1);
        }

        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetIndex(int requestedCount)
        {
#if CHECKED
            if (position + requestedCount > size)
            {
                throw new IOException("Cannot write outside of boundaries of memory buffer");
            }
#endif
            return offset + position;
        }
    }
}