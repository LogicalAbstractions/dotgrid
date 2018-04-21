using System;

namespace DotGrid.Core.Pooling
{
    public readonly struct ObjectPoolHandle<T> : IDisposable
    {
        public readonly IObjectPool<T> Owner;
        public readonly T Value;

        internal ObjectPoolHandle(IObjectPool<T> owner, T value)
        {
            Owner = owner ?? throw new ArgumentNullException(nameof(owner));
            Value = value;
        }

        public void Dispose()
        {
            Owner.Release(Value);
        }

        public static implicit operator T(ObjectPoolHandle<T> handle)
        {
            return handle.Value;
        }
    }

    public static class ObjectPoolExtensions
    {
        public static ObjectPoolHandle<T> AquireHandle<T>(this IObjectPool<T> objectPool)
        {
            if (objectPool == null) throw new ArgumentNullException(nameof(objectPool));
            return new ObjectPoolHandle<T>(objectPool, objectPool.Aquire());
        }
    }
}