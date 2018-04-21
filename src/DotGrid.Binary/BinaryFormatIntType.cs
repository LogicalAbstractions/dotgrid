using System.Runtime.CompilerServices;

namespace DotGrid.Binary
{
    internal enum BinaryFormatPropertyIdType : byte
    {
        Byte = 0x00,
        Short,
        Int,
    }
    
    internal static class BinaryFormatPropertyIdExtensions
    {
        internal static BinaryFormatPropertyIdType DetectPropertyIdType(int propertyCount)
        {
            if (propertyCount < byte.MaxValue)
            {
                return BinaryFormatPropertyIdType.Byte;
            }

            if (propertyCount < short.MaxValue)
            {
                return BinaryFormatPropertyIdType.Short;
            }

            return BinaryFormatPropertyIdType.Int;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int ReadPropertyIdType(this BinaryFormatMemoryReader reader,
            BinaryFormatPropertyIdType type)
        {
            switch (type)
            {
                case BinaryFormatPropertyIdType.Byte:
                    return reader.ReadByte();
                case BinaryFormatPropertyIdType.Short:
                    return reader.ReadShort();
                case BinaryFormatPropertyIdType.Int:
                    return reader.ReadInt();
            }

            return reader.ReadInt();
        }
    }
}