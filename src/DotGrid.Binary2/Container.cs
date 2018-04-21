using System.Collections.Generic;

namespace DotGrid.Binary2
{
    internal sealed class Container
    {  
        // TODOPERF: Replacement with array or span could yield benefits ( Span for ref returns, and array
        // iteration is also faster ) 
        // TODOPERF: Merge the multiple entries iterations into one.
        private readonly List<ContainerEntry> entries = new List<ContainerEntry>();
        private ValueType containerType;
        private int startPosition;
        private int lastPropertyId;

        internal ValueType ContainerType => containerType;

        internal int LastPropertyId => lastPropertyId;

        internal ContainerEntry First()
        {
            return entries[0];
        }
        
        internal void PushArrayElement(int position,ValueType valueType,int parentPropertyId)
        {
            entries.Add(new ContainerEntry(position,entries.Count,valueType,parentPropertyId));
        }

        internal void PushObjectProperty(int position,int propertyId,ValueType valueType,int parentPropertyId)
        {
            entries.Add(new ContainerEntry(position,propertyId,valueType,parentPropertyId));
        }
        
        internal void Reset(ValueType containerType,int startPosition,int lastPropertyId)
        {
            this.containerType = containerType;
            this.startPosition = startPosition;
            this.lastPropertyId = lastPropertyId;
            
            entries.Clear();
        }

        internal void WriteFooter(MemoryWriter writer)
        {
            var metadataStartPosition = writer.Position;
            
            CalculateOffsets(metadataStartPosition);
            
            switch (containerType)
            {
                case ValueType.Array:
                    WriteArrayFooter(writer);
                    return;
                case ValueType.Object:
                    WriteObjectFooter(writer);
                    return;
            }
        }

        private void WriteArrayFooter(MemoryWriter writer)
        {
            var offsetType       = DetectOffsetType();
            //var entriesCountType = entries.Count.DetectedFixedIntegerType();
            
            // TODOPERF: Merge these two values into one byte
            writer.WriteByte((byte)offsetType);
            //writer.WriteByte((byte)entriesCountType);
            
            writer.WriteInt(entries.Count);
            //writer.WriteFixedInteger(entriesCountType,entries.Count);

            for (int i = 0; i < entries.Count; ++i)
            {
                writer.WriteFixedInteger(offsetType,entries[i].PositionOrOffset);
                writer.WriteByte((byte)entries[i].ValueType);
            }
        }

        private void WriteObjectFooter(MemoryWriter writer)
        {
            var offsetType     = DetectOffsetType();
            var propertyIdType = DetectPropertyIdType();
            //var entriesCountType = entries.Count.DetectedFixedIntegerType();
            
            // TODOPERF: Merge these three values into one byte
            writer.WriteByte((byte)offsetType);
            writer.WriteByte((byte)propertyIdType);
            //writer.WriteByte((byte)entriesCountType);
            
            writer.WriteInt(entries.Count);
            //writer.WriteFixedInteger(entriesCountType,entries.Count);
            
            SortEntries();

            for (int i = 0; i < entries.Count; ++i)
            {
                writer.WriteFixedInteger(offsetType,entries[i].PositionOrOffset);
                writer.WriteFixedInteger(propertyIdType,entries[i].PropertyIdOrArrayIndex);
                writer.WriteByte((byte)entries[i].ValueType);
            }
        }

        private void CalculateOffsets(int relativeTo)
        {
            for (int i = 0; i < entries.Count; ++i)
            {
                var currentEntry = entries[i];
                
                entries[i] = new ContainerEntry(relativeTo - currentEntry.PositionOrOffset,
                    currentEntry.PropertyIdOrArrayIndex,
                    currentEntry.ValueType,currentEntry.ParentPropertyId);
            }
        }

        private FixedIntegerType DetectPropertyIdType()
        {
            int maxPropertyId = 0;

            for (int i = 0; i < entries.Count; ++i)
            {
                if (entries[i].PropertyIdOrArrayIndex > maxPropertyId)
                {
                    maxPropertyId = entries[i].PropertyIdOrArrayIndex;
                }
            }

            return maxPropertyId.DetectedFixedIntegerType();
        }

        private FixedIntegerType DetectOffsetType()
        {
            int maxOffset = 0;

            for (int i = 0; i < entries.Count; ++i)
            {
                if (entries[i].PositionOrOffset > maxOffset)
                {
                    maxOffset = entries[i].PositionOrOffset;
                }
            }

            return maxOffset.DetectedFixedIntegerType();
        }

        private void SortEntries()
        {
            if (entries.Count > 10)
            {
                entries.Sort(new ContainerEntryComparer());
            }
            else
            {
                BubbleSortEntriesByPropertyId();
            }
        }
        
        /// <summary>
        /// We use a custom bubble sort instead of builtin sort here
        /// Usually we have only a handful properties and on those small
        /// sets BubbleSort provides better cache locality and complexity
        /// is irellevant.
        /// </summary>
        private void BubbleSortEntriesByPropertyId()
        {
            var n = entries.Count;
            do
            {
                int newn = 0;

                for (int i = 1; i < n; ++i)
                {
                    var a1 = entries[i - 1];
                    var a2 = entries[i];

                    if (a1.PositionOrOffset > a2.PositionOrOffset)
                    {
                        // Swap
                        entries[i] = a1;
                        entries[i - 1] = a2;
                        newn = i;
                    }
                }

                n = newn;
            } while (n > 0);
        }
    }
}