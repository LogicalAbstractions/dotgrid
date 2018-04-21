using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace DotGrid.Core.Pooling
{
    public class ConcurrentObjectPool<T> : IObjectPool<T>
    {
        private readonly Func<T> factoryFunc;
        private readonly Action<T> releaseFunc;
        private readonly ConcurrentQueue<T> items = new ConcurrentQueue<T>();

        internal ConcurrentObjectPool(Func<T> factoryFunc, Action<T> releaseFunc,int initialSize)
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
            if (items.TryDequeue(out var result))
            {
                return result;
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