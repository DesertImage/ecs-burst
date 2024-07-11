using DesertImage.Collections;
using Unity.Jobs;

namespace DesertImage.ECS
{
    public unsafe struct RemoveComponentSystem<T> : IInitialize, IExecute where T : unmanaged
    {
        private EntitiesGroup _group;

        public void Initialize(in World world)
        {
            _group = Filter.Create(world).With<T>().Find();
        }

        public void Execute(ref SystemsContext context)
        {
            var job = new RemoveComponentJob
            {
                Entities = _group.Values,
                World = context.World
            };

            context.Handle = job.Schedule(context.Handle);
        }

        private struct RemoveComponentJob : IJob
        {
            public UnsafeReadOnlyArray<uint> Entities;
            public World World;

            public void Execute()
            {
                for (var i = 0; i < Entities.Length; i++)
                {
                    var entity = new Entity(Entities[i], World.Ptr);
                    entity.Remove<T>();
                }
            }
        }
    }
}