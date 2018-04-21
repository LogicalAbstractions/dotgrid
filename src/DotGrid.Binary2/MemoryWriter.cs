using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Utf8;

namespace DotGrid.Binary2
{
    public sealed unsafe class MemoryWriter 
    {
        private readonly byte* targetPtr;
        private readonly int size;     
        private int position;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MemoryWriter(byte* targetPtr, int size)
        {          
            this.targetPtr = targetPtr;
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
        public void WriteByte(byte value)
        {
            *(targetPtr + position) = value;
            Seek(position + 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteBytes(in ReadOnlySpan<byte> buffer)
        {
            var index = position;
            var spanSize = size - index;
            var targetSpan = new Span<byte>(targetPtr + position,spanSize);
            
            buffer.CopyTo(targetSpan);
            Seek(position + buffer.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteShort(short value)
        {
            var ptr = (short*) (targetPtr + position);
            *ptr = value;
            
            Seek(position + 2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteInt(int value)
        {
            var ptr = (int*) (targetPtr + position);
            *ptr = value;
            
            Seek(position + 4);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteLong(long value)
        {
            var ptr = (long*) (targetPtr + position);
            *ptr = value;
            
            Seek(position + 8);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteFloat(float value)
        {
            var ptr = (float*) (targetPtr + position);
            *ptr = value;
            
            Seek(position + 4);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteDouble(double value)
        {
            var ptr = (double*) (targetPtr + position);
            *ptr = value;
            
            Seek(position + 8);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteBoolean(bool value)
        {
            var ptr = targetPtr + position;
            *ptr = (byte) (value ? 1 : 0);
            
            Seek(position + 1);
        }
    }

    public static class MemoryWriterExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteUtf8Span(this MemoryWriter writer, in Utf8Span value)
        {
            var bytes = value.Bytes;
            writer.WriteUnsignedVarInt((uint)bytes.Length);
            writer.WriteBytes(bytes);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteSignedVarInt(this MemoryWriter writer, int value)
        {
            writer.WriteUnsignedVarInt(EncodeZigZag32(value));
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteUnsignedVarInt(this MemoryWriter writer, uint value)
        {
            // Optimize for the common case of a single byte value
            if (value < 128)
            {
                writer.WriteByte((byte)value);
                return;
            }

            while (value > 127)
            {
                var byteValue = (byte) ((value & 0x7F) | 0x80);
                writer.WriteByte(byteValue);
                value >>= 7;
            }
            while (value > 127)
            {
                writer.WriteByte((byte) ((value & 0x7F) | 0x80));
                value >>= 7;
            }
            
            writer.WriteByte((byte)value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteSignedVarLong(this MemoryWriter writer, long value)
        {
            writer.WriteUnsignedVarLong(EncodeZigZag64(value));
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteUnsignedVarLong(this MemoryWriter writer,ulong value) 
        {
            while (value > 127)
            {
                writer.WriteByte( (byte) ((value & 0x7F) | 0x80));
                value >>= 7;
            }
            while (value > 127)
            {
                writer.WriteByte((byte) ((value & 0x7F) | 0x80));
                value >>= 7;
            }
            
            writer.WriteByte((byte)value);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint EncodeZigZag32(int n)
        {
            return (uint) ((n << 1) ^ (n >> 31));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong EncodeZigZag64(long n)
        {
            return (ulong) ((n << 1) ^ (n >> 63));
        }
    }
}