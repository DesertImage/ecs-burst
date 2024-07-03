using DesertImage.Assets;
using Unity.Mathematics;

namespace DesertImage.ECS
{
    public struct ViewsSystem : IInitialize, IExecute, IDestroy
    {
        private EntitiesGroup _group;
        private EntitiesGroup _destroyGroup;

        public void Initialize(in World world)
        {
            _group = Filter.Create(world)
                .With<InstantiateView>()
                .Find();

            _destroyGroup = Filter.Create(world)
                .With<View>()
                .With<DestroyView>()
                .Find();
        }

        public void Execute(ref SystemsContext context)
        {
            ref var viewTransforms = ref context.World.GetStatic<ViewTransforms>();
            var views = _group.GetComponents<View>();
            var instantiateViews = _group.GetComponents<InstantiateView>();

            foreach (var entityId in _group)
            {
                var entity = _group.GetEntity(entityId);

                var instantiateView = instantiateViews.Read(entityId);

                var view = context.World.GetModule<SpawnManager>().SpawnAs<EntityView>(instantiateView.Id);

                var transform = view.transform;

                transform.position = instantiateView.Position;
                transform.rotation = quaternion.Euler(instantiateView.Rotation);

                view.Initialize(entity);
#if UNITY_EDITOR
                view.name = $"Entity {entityId}";
#endif
                viewTransforms.Values.Add(transform);
                viewTransforms.Indexes.Add(entityId, viewTransforms.Indexes.Count);

                entity.Replace(new View { Value = view });
            }

            foreach (var entityId in _destroyGroup)
            {
                var entity = _group.GetEntity(entityId);

                var view = views.Read(entityId).Value.Value;

                entity.Remove<View>();
                context.World.GetModule<SpawnManager>().Release(view);

                viewTransforms.Values.RemoveAtSwapBack(viewTransforms.Indexes.Read(entityId));
            }
        }

        public void OnDestroy(in World world)
        {
            var viewTransforms = world.GetStatic<ViewTransforms>();
            viewTransforms.Values.Dispose();
            viewTransforms.Indexes.Dispose();
        }
    }
}