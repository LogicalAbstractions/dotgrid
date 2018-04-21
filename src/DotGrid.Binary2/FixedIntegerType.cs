using System.Runtime.CompilerServices;

namespace DotGrid.Binary2
{
    internal enum FixedIntegerType : byte
    {
        Byte   = 0x01,
        Short  = 0x02,
        Int    = 0x04
    }
    
    internal static class FixedIntegerTypeExtensions 
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static FixedIntegerType DetectedFixedIntegerType(this int maxValue)
        {
            if (maxValue < byte.MaxValue)
            {
                return FixedIntegerType.Byte;
            }

            if (maxValue < short.MaxValue)
            {
                return FixedIntegerType.Short;
            }

            return FixedIntegerType.Int;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int ReadFixedInteger(this MemoryReader reader,FixedIntegerType type)
        {
            switch (type)
            {
                case FixedIntegerType.Byte:
                    return reader.ReadByte();
                case FixedIntegerType.Short:
                    return reader.ReadShort();
                case FixedIntegerType.Int:
                    return reader.ReadInt();
            }

            return reader.ReadInt();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void WriteFixedInteger(this MemoryWriter writer, FixedIntegerType type, int value)
        {
            switch (type)
            {
                case FixedIntegerType.Byte:
                    writer.WriteByte((byte)value);
                    return;
                case FixedIntegerType.Short:
                    writer.WriteShort((short)value);
                    return;
                case FixedIntegerType.Int:
                    writer.WriteInt(value);
                    return;
            }
        }
    }
}