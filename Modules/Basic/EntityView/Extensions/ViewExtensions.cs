using System;
using Unity.Mathematics;

namespace DesertImage.ECS
{
    public unsafe static class ViewExtensions
    {
        public static void InstantiateView(this in Entity entity, uint id)
        {
#if DEBUG_MODE
            if (!entity.IsAlive()) throw new Exception("Entity is not alive");
#endif
            entity.Replace
            (
                new InstantiateView
                {
                    Id = id
                }
            );
        }
        
        public static void InstantiateView(this in Entity entity, uint id, float3 position)
        {
#if DEBUG_MODE
            if (!entity.IsAlive()) throw new Exception("Entity is not alive");
#endif
            entity.Replace
            (
                new InstantiateView
                {
                    Id = id,
                    Position = position
                }
            );
        }
        
        public static void InstantiateView(this in Entity entity, uint id, float3 position, float3 rotation)
        {
#if DEBUG_MODE
            if (!entity.IsAlive()) throw new Exception("Entity is not alive");
#endif
            entity.Replace
            (
                new InstantiateView
                {
                    Id = id,
                    Position = position,
                    Rotation = rotation
                }
            );
        }

        public static void DestroyView(this in Entity entity)
        {
#if DEBUG_MODE
            if (!entity.IsAlive()) throw new Exception("Entity is not alive");
            if (!entity.Has<View>()) throw new Exception("Entity has not view");
#endif
            entity.Replace<DestroyView>();
        }
    }
}