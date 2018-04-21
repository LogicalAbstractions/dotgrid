using System.Collections.Generic;

namespace DotGrid.Binary2
{
    internal readonly struct ContainerEntry
    {
        internal readonly int PositionOrOffset;

        internal readonly int PropertyIdOrArrayIndex;

        internal readonly ValueType ValueType;

        internal readonly int ParentPropertyId;

        internal ContainerEntry(int positionOrOffset, int propertyIdOrArrayIndex, ValueType valueType, int parentPropertyId)
        {
            PositionOrOffset = positionOrOffset;
            PropertyIdOrArrayIndex = propertyIdOrArrayIndex;
            ValueType = valueType;
            ParentPropertyId = parentPropertyId;
        }
    }

    internal readonly struct ContainerEntryComparer : IComparer<ContainerEntry>
    {
        public int Compare(ContainerEntry x, ContainerEntry y)
        {
            if (x.PropertyIdOrArrayIndex < y.PropertyIdOrArrayIndex) return -1;
            if (x.PropertyIdOrArrayIndex > y.PropertyIdOrArrayIndex) return 1;
            return 0;
        }
    }
}