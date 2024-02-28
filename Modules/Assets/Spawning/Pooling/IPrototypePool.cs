namespace DesertImage.Assets
{
    public interface IPool
    {
        void Clear();
    }

    public interface IPrototypePool<T> : IPool where T : IPoolable
    {
        void Register(uint id, T instance, int preRegisteredCount);
        T Get(uint id);
        void Release(T instance);
    }
}