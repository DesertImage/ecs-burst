namespace DesertImage.ECS
{
    public struct ComponentTypes<T>
    {
        public static readonly Unity.Burst.SharedStatic<uint> TypeId =
            Unity.Burst.SharedStatic<uint>.GetOrCreate<ComponentTypes<T>>();
    }
}