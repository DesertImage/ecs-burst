using System;
using DesertImage.Assets;

namespace DesertImage.ECS
{
    public unsafe static class ViewExtensions
    {
        public static EntityView InstantiateView(this ref Entity entity, uint id)
        {
#if DEBUG_MODE
            if (!entity.IsAlive()) throw new Exception("Entity is not alive");
#endif
            var view = entity.World->GetModule<SpawnManager>().SpawnAs<EntityView>(id);
            view.Initialize(entity);

            return view;
        }

        public static void DestroyView(this ref Entity entity)
        {
#if DEBUG_MODE
            if (!entity.IsAlive()) throw new Exception("Entity is not alive");
            if (!entity.Has<View>()) throw new Exception("Entity has not view");
#endif
            var view = entity.Get<View>().Value.Value;
            
            entity.Remove<View>();
            entity.World->GetModule<SpawnManager>().Release(view);
        }
    }
}