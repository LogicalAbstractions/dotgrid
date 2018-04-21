using System.Collections.Generic;
using System.Linq;
using System.Text.Utf8;

namespace DotGrid.Binary2
{
    internal sealed class PropertyNamesWriter
    {
        private readonly Dictionary<string,int> propertyIds;
        private readonly List<string> propertyNames;
        private readonly List<int> propertyNameOffsets;
        private readonly bool isPrePopulated;

        internal PropertyNamesWriter(IReadOnlyDictionary<string, int> propertyIds)
        {
            this.propertyIds = propertyIds.ToDictionary(e => e.Key, e => e.Value);
            this.propertyNameOffsets = new List<int>(propertyIds.Count);
            this.propertyNames = new List<string>(propertyIds.Count);
            this.isPrePopulated = true;
        }

        internal PropertyNamesWriter()
        {
            this.propertyIds = new Dictionary<string, int>();
            this.propertyNameOffsets = new List<int>();
            this.propertyNames = new List<string>();
            this.isPrePopulated = false;
        }

        internal IReadOnlyDictionary<string, int> PropertyIds => propertyIds;

        internal int GetOrAdd(string propertyName)
        {
            if (!propertyIds.TryGetValue(propertyName, out var propertyId))
            {
                if (!isPrePopulated)
                {
                    propertyIds.Add(propertyName,propertyIds.Count);
                    propertyNames.Add(propertyName);
                    propertyNameOffsets.Add(0);
                    
                    return propertyIds.Count - 1;
                }
                
                throw new BinaryFormatException("Cannot define new property names");
            }

            return propertyId;
        }

        internal bool WritePropertyNames(MemoryWriter writer)
        {
            if (!isPrePopulated)
            {
                var metadataStartPosition = writer.Position;

                for (int i = 0; i < propertyNames.Count; ++i)
                {
                    var namePosition = writer.Position;

                    propertyNameOffsets[i] = namePosition - metadataStartPosition;
                    
                    writer.WriteUtf8Span((Utf8Span)propertyNames[i]);
                }

                //var propertyNameOffsetType = propertyNameOffsets.Count.DetectedFixedIntegerType();

                for (int i = 0; i < propertyNameOffsets.Count; ++i)
                {
                    writer.WriteInt(propertyNameOffsets[i]);
                }

                writer.WriteInt(propertyNames.Count);
                //writer.WriteFixedInteger(propertyNameOffsetType,propertyNameOffsets.Count);
                //writer.WriteByte((byte)propertyNameOffsetType);
                
                return true;
            }

            return false;
        }

        internal void Reset()
        {
            if (!isPrePopulated)
            {
                propertyIds.Clear();
                propertyNames.Clear();
                propertyNameOffsets.Clear();
            }
        }
    }
}