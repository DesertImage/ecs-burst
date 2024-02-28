using System.Collections.Generic;
using UnityEngine;

namespace DesertImage.ECS
{
    public abstract class EcsStarter : ModuleProvider
    {
        protected World World;

        [SerializeField] private List<ScriptableObject> modules;

        private readonly Dictionary<int, object> _allModules = new Dictionary<int, object>();
        private readonly List<IUpdate> _updatables = new List<IUpdate>();
        private readonly List<IDestroy> _destroyables = new List<IDestroy>();

        protected virtual void Awake()
        {
            World = Worlds.Create(this);
            Initialize();
        }

        protected virtual void OnDestroy()
        {
            for (var i = _destroyables.Count - 1; i >= 0; i--)
            {
                _destroyables[i].OnDestroy();
            }

            World.Dispose();
        }

        private void Update()
        {
            var deltaTime = Time.deltaTime;

            World.Tick(deltaTime);

            for (var i = 0; i < _updatables.Count; i++)
            {
                _updatables[i].OnUpdate(deltaTime);
            }
        }

        protected virtual void Initialize()
        {
            InitModules();
            InitSystems();
        }

        protected virtual void InitModules() => AddModules(modules);

        protected abstract void InitSystems();


        protected void AddModule<T>(T module) where T : class
        {
            if (module is IAwake awakable) awakable.OnAwake(in World);
            if (module is IUpdate updatable) _updatables.Add(updatable);
            if (module is IDestroy destroyable) _destroyables.Add(destroyable);

            _allModules.Add(module.GetType().GetHashCode(), module);
        }

        private void AddModules(IEnumerable<object> values)
        {
            foreach (var module in values)
            {
                AddModule(module);
            }
        }

        public override T Get<T>()
        {
            if (!_allModules.TryGetValue(typeof(T).GetHashCode(), out var module)) return default;
            return (T)module;
        }
    }
}