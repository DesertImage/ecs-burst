namespace DesertImage.Assets
{
    public partial class SpawnManager
    {
        internal static class ObjectTypeCounter
        {
            private static uint _idCounter;

            public static uint GetId<T>()
            {
                if (ObjectType<T>.Id > 0) return ObjectType<T>.Id;
                ObjectType<T>.Id = ++_idCounter;
                return _idCounter;
            }
        }

        // ReSharper disable once ClassNeverInstantiated.Global
        // ReSharper disable once UnusedTypeParameter
        public sealed class ObjectType<T>
        {
            // ReSharper disable once StaticMemberInGenericType
            public static uint Id;
        }
    }
}