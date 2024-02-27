using System;
using Object = UnityEngine.Object;

namespace DesertImage.ECS
{
    [Serializable]
    public struct ObjectReference<T> where T : Object
    {
        public uint Id;
        public T Value => ObjectsReferenceRegistry.GetStorage().Get<T>(ref Id, null);

        private ObjectReference(T obj)
        {
            Id = 0;
            ObjectsReferenceRegistry.GetStorage().Get<T>(ref Id, obj);
        }

        public static implicit operator ObjectReference<T>(T obj) => new ObjectReference<T>(obj);
        public static implicit operator T(ObjectReference<T> reference) => reference.Value;
    }
}