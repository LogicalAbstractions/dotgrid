using System;
using System.Collections.Generic;

namespace DotGrid.Binary2
{
    internal sealed class ContainerPool
    {
        private readonly Queue<Container> items = new Queue<Container>();

        internal ContainerPool(int initialSize)
        {
            for (int i = 0; i < initialSize; ++i)
            {
                items.Enqueue(new Container());
            }
        }

        internal Container Aquire(ValueType containerType,int startPosition,int lastPropertyId)
        {
            Container result = null;
            
            result = items.Count > 0 ? items.Dequeue() : new Container();

            result.Reset(containerType,startPosition,lastPropertyId);

            return result;
        }

        internal void Release(Container container)
        {
            items.Enqueue(container);
        }
    }
}