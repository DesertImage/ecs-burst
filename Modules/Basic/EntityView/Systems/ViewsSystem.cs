using DesertImage.Assets;
using Unity.Mathematics;

namespace DesertImage.ECS
{
    public struct ViewsSystem : IInitialize, IExecute, IDestroy
    {
        private EntitiesGroup _group;
        private EntitiesGroup _addGroup;
        private EntitiesGroup _removeGroup;
        private EntitiesGroup _destroyGroup;

        public void Initialize(in World world)
        {
            _group = Filter.Create(world)
                .With<InstantiateView>()
                .Find();

            _addGroup = Filter.Create(world)
                .With<ViewAdd>()
                .Find();

            _removeGroup = Filter.Create(world)
                .With<ViewRemove>()
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
            var viewAdds = _group.GetComponents<ViewAdd>();
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

                entity.Replace(new ViewAdd { Value = transform });
                entity.Replace(new View { Value = view });
            }

            foreach (var entityId in _addGroup)
            {
                var viewAdd = viewAdds.Read(entityId);

                viewTransforms.Values.Add(viewAdd.Value);
                viewTransforms.Indexes.Add(entityId, viewTransforms.Indexes.Count);
                
                _addGroup.GetEntity(entityId).Remove<ViewAdd>();
            }

            foreach (var entityId in _removeGroup)
            {
                viewTransforms.Values.RemoveAtSwapBack(viewTransforms.Indexes.Read(entityId));
                _removeGroup.GetEntity(entityId).Remove<ViewRemove>();
            }

            foreach (var entityId in _destroyGroup)
            {
                var entity = _group.GetEntity(entityId);

                var view = views.Read(entityId).Value.Value;

                entity.Remove<View>();
                context.World.GetModule<SpawnManager>().Release(view);

                entity.Replace<ViewRemove>();
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