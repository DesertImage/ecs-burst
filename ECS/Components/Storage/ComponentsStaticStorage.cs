namespace DesertImage.ECS
{
    public class ComponentsStaticStorage<T> : ComponentsStorageBase
    {
        public T Data;
        
        public override void Dispose() => Data = default;
    }
}