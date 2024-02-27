namespace DesertImage.Assets
{
    public interface ILibrary
    {
    }

    public interface ILibrary<TId, TItem> : ILibrary
    {
        void Register(TId id, TItem item);
        TItem Get(TId id);
    }
}