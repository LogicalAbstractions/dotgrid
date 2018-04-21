namespace DotGrid.Core.Pooling
{
    public interface IObjectPool<T>
    {
        T Aquire();

        void Release(T obj);
    }
}