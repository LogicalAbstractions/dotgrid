using System;
using System.Buffers.Text;
using System.Drawing;
using System.Text;

namespace DotGrid.Core.Memory
{
    public readonly unsafe struct MemorySegment
    {
        public static readonly MemorySegment Null = new MemorySegment(null,0);
        
        public readonly byte* Pointer;

        public readonly ulong Size;

        public MemorySegment(byte* pointer, ulong size)
        {
            Pointer = pointer;
            Size = size;
        }

        public bool IsNull => Pointer == null;

        public bool IsNotNull => Pointer != null;
    }

    public static unsafe class StringExtensions
    {
        public static void AsMemory(this string value, Action<MemorySegment> action,Encoding encoding = null)
        {
            if (value == null)
            {
                action.Invoke(MemorySegment.Null);
                return;
            }
            
            var finalEncoding = encoding ?? Encoding.UTF8;

            var data = finalEncoding.GetBytes(value);

            fixed (byte* ptr = &data[0])
            {
                action.Invoke(new MemorySegment(ptr,(ulong)data.Length));
            }
        }

        public static string DecodeToString(this in MemorySegment value, Encoding encoding = null)
        {
            var finalEncoding = encoding ?? Encoding.UTF8;

            if (value.IsNull)
            {
                return null;
            }

            return finalEncoding.GetString(value.Pointer, (int) value.Size);
        }
    }
}