using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace DotGrid.Binary2
{
    internal sealed class PropertyNamesReader
    {
        private readonly Dictionary<string, int> propertyIdsByName;
        private readonly Dictionary<int, string> propertyNamesById;
        private readonly MemoryReader reader;
        private readonly int metadataPosition;

        internal PropertyNamesReader(MemoryReader reader,int metadataPosition)
        {
            this.reader             = reader;
            this.metadataPosition   = metadataPosition;
  
            this.propertyIdsByName  = new Dictionary<string, int>();
            this.propertyNamesById  = new Dictionary<int, string>();
        }
        
        internal PropertyNamesReader( MemoryReader reader,int metadataPosition,IReadOnlyDictionary<string, int> propertyIds)
        {
            this.reader            = reader;
            this.metadataPosition  = metadataPosition;
           
            this.propertyIdsByName = propertyIds.ToDictionary(p => p.Key, p => p.Value);
            this.propertyNamesById = propertyIds.ToDictionary(p => p.Value, p => p.Key);
        }
        
        internal int GetPropertyIdForName(string propertyName)
        {
            if (propertyIdsByName.TryGetValue(propertyName, out var propertyId))
            {
                return propertyId;
            }
            
            // Now do a search:
            if (metadataPosition >= 0)
            {
                MoveToPropertyNameOffsets(out var nameCount,out var startPosition);

                for (int i = 0; i < nameCount; ++i)
                {
                    reader.Seek(CalculateOffsetPosition(startPosition,i));
                    
                    var nameOffset = reader.ReadInt();
                    var namePosition = startPosition + nameOffset;
                    
                    reader.Seek(namePosition);

                    var name = reader.ReadUtf8Span().ToString();

                    propertyNamesById[i] = name;
                    propertyIdsByName[name] = i;
                    
                    if (name.Equals(propertyName))
                    {
                        return i;
                    }
                }
            }
            
            throw new BinaryFormatException($"Property {propertyName} not found");
        }

        internal string GetPropertyNameForId(int propertyId)
        {
            if (propertyNamesById.TryGetValue(propertyId, out var propertyName))
            {
                return propertyName;
            }

            if (metadataPosition >= 0)
            {
                MoveToPropertyNameOffsets(out var nameCount,out var startPosition);

                reader.Seek(CalculateOffsetPosition(startPosition,propertyId));

                var nameOffset = reader.ReadInt();
                
                reader.Seek(startPosition + metadataPosition);

                var name = reader.ReadUtf8Span().ToString();

                propertyIdsByName[name] = propertyId;
                propertyNamesById[propertyId] = name;

                return name;
            }

            throw new BinaryFormatException($"Property {propertyId} not found");
        }

        private void MoveToPropertyNameOffsets(out int nameCount, out int startPosition)
        {
            reader.Seek(metadataPosition - sizeof(int));

            int entryCount = reader.ReadInt();

            var offsetsStart = metadataPosition - entryCount * sizeof(int) - sizeof(int);

            reader.Seek(offsetsStart);

            startPosition = offsetsStart;
            nameCount = entryCount;
        }

        private int CalculateOffsetPosition(int startPosition, int index)
        {
            return startPosition + index * sizeof(int);
        }  
    }
}