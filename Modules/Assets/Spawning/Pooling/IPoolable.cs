namespace DesertImage.Assets
{
    public interface IPoolable
    {
        uint PoolableId { get; }

        void OnUnpool(uint id);
        void OnRelease();
    }
}