using System;
using System.Collections.Generic;
using System.Text.Utf8;

namespace DotGrid.Binary
{
    public sealed class BinaryFormatReadingContext
    {
        internal struct ContainerEntry
        {
            internal int             Position;
            internal BinaryFormatValueType  ContainerType;
        }
        
        private readonly Dictionary<Utf8String,int> propertyIds = new Dictionary<Utf8String, int>();
        private readonly Dictionary<int,Utf8String> propertyNames = new Dictionary<int, Utf8String>();
        private readonly Stack<ContainerEntry> containerStack = new Stack<ContainerEntry>();
          
        internal void Reset()
        {
            propertyIds.Clear();
            propertyNames.Clear();
            containerStack.Clear();
        }

        internal bool TryGetPropertyName(int id, out Utf8String name)
        {
            return propertyNames.TryGetValue(id, out name);
        }
        
        internal bool TryGetPropertyId(Utf8String name, out int id)
        {
            return propertyIds.TryGetValue(name, out id);
        }

        internal void SetPropertyName(int id, Utf8String name)
        {
            propertyNames[id] = name;
        }

        internal void SetPropertyId(Utf8String name, int id)
        {
            propertyIds[name] = id;
        }

        internal void ClearContainers()
        {
            containerStack.Clear();
        }
        
        internal void PushContainer(BinaryFormatValueType containerType, int position)
        {
            containerStack.Push(new ContainerEntry() { ContainerType = containerType,Position = position});
        }

        internal bool TryPopContainer(out ContainerEntry currentEntry)
        {
            try
            {
                containerStack.Pop();
                currentEntry = containerStack.Peek();

                return true;
            }
            catch (InvalidOperationException)
            {
                currentEntry = new ContainerEntry();
                return false;
            }
        }
    }
}