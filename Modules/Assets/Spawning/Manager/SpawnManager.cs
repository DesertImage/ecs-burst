using UnityEngine;

namespace DesertImage.Assets
{
    public partial class SpawnManager
    {
        private readonly PrototypePool<MonoBehaviourPoolable> _pool;

        public SpawnManager()
        {
            _pool = new PrototypePool<MonoBehaviourPoolable>(Object.Instantiate);
        }

        public void Register(uint id, MonoBehaviourPoolable poolable, int preRegisteredCount)
        {
            _pool.Register(id, poolable, preRegisteredCount);
        }

        public MonoBehaviourPoolable Spawn(uint id) => _pool.Get(id);

        public T SpawnAs<T>(uint id) where T : MonoBehaviourPoolable => (T)_pool.Get(id);

        public void Release(MonoBehaviourPoolable obj) => _pool.Release(obj);
    }
}