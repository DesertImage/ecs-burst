using DesertImage.Collections;
using UnityEngine.Jobs;

namespace DesertImage.ECS
{
    public struct EntityViewFeature : IFeature
    {
        public void Link(World world)
        {
            world.ReplaceStatic
            (
                new ViewTransforms
                {
                    Values = new TransformAccessArray(20),
                    Indexes = new UnsafeUintSparseSet<int>(20)
                }
            );

            world.Add<ViewsSystem>();

            world.AddRemoveComponentSystem<InstantiateView>();
            world.AddRemoveComponentSystem<DestroyView>();
        }
    }
}