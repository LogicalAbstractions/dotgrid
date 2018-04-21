using System;

namespace DotGrid.Core.Pooling
{
    public static class ObjectPools
    {
        public static IObjectPool<T> SingleThreaded<T>(Func<T> factoryFunc, Action<T> releaseFunc = null,
            int initialSize = 16)
        {
            return new SingleThreadedObjectPool<T>(factoryFunc, releaseFunc ?? (obj => {}), initialSize);
        }
        
        public static IObjectPool<T> Concurrent<T>(Func<T> factoryFunc, Action<T> releaseFunc = null,
            int initialSize = 16)
        {
            return new ConcurrentObjectPool<T>(factoryFunc, releaseFunc ?? (obj => {}), initialSize);
        }
    }
}