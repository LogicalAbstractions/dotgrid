using System;
using System.Collections.Generic;

namespace DotGrid.Core.Pooling
{
    public class SingleThreadedObjectPool<T> : IObjectPool<T>
    {
        private readonly Func<T> factoryFunc;
        private readonly Action<T> releaseFunc;
        private readonly Queue<T> items = new Queue<T>();

        internal SingleThreadedObjectPool(Func<T> factoryFunc, Action<T> releaseFunc,int initialSize)
        {
            this.factoryFunc = factoryFunc;
            this.releaseFunc = releaseFunc;

            for (int i = 0; i < initialSize; ++i)
            {
                items.Enqueue(factoryFunc.Invoke());
            }
        }

        public T Aquire()
        {
            if (items.Count > 0)
            {
                return items.Dequeue();
            }

            return factoryFunc.Invoke();
        }

        public void Release(T obj)
        {
            releaseFunc.Invoke(obj);
            items.Enqueue(obj);
        }
    }
}