using System;
using System.Reflection;
using Unity.Burst;
using Unity.Collections;

namespace DesertImage.ECS
{
    public unsafe struct SystemsManager : IDisposable
    {
        private SystemsState _state;

        private EntitiesManager* _entitiesManager;
        private GroupsManager* _groupsManager;

        public SystemsManager(EntitiesManager* entitiesManager, GroupsManager* groupsManager)
        {
            _entitiesManager = entitiesManager;
            _groupsManager = groupsManager;

            _state = new SystemsState
            {
                ExecuteSystems = new UnsafeList<ExecuteSystemData>(20, Allocator.Persistent),
                DestroySystems = new UnsafeList<FunctionPointer<SystemsTools.Destroy>>(20, Allocator.Persistent),
                SystemsHash = new UnsafeHashSet<int>(20, Allocator.Persistent)
            };
        }

        public unsafe void Add<T>() where T : unmanaged, ISystem
        {
            var systemType = typeof(T);

            if (Has<T>())
            {
#if DEBUG
                throw new Exception($"system {systemType} already added");
#endif
                return;
            }

            ref var state = ref _state;

            var isAwake = typeof(IAwake).IsAssignableFrom(systemType);
            if (isAwake)
            {
                var methodInfo = typeof(SystemsManager).GetMethod
                (
                    nameof(AddAwake),
                    BindingFlags.Static | BindingFlags.NonPublic
                );

                var gMethod = methodInfo.MakeGenericMethod(systemType);

                var targetDelegate = Delegate.CreateDelegate(typeof(Action), default, gMethod);
                var converted = (Action)targetDelegate;

                converted.Invoke();
            }

            var isExecute = typeof(IExecuteSystem).IsAssignableFrom(systemType);
            if (isExecute)
            {
                var wrapper = new ExecuteSystemWrapper
                {
                    Value = MemoryTools.Allocate(default(T)),
                };

                var wrapperPtr = (ExecuteSystemWrapper*)MemoryTools.Allocate(wrapper);

                var methodInfo = typeof(SystemsManager).GetMethod
                (
                    nameof(AddExecute),
                    BindingFlags.Static | BindingFlags.NonPublic
                );

                var gMethod = methodInfo.MakeGenericMethod(systemType);

                var targetDelegate = Delegate.CreateDelegate(typeof(Action<IntPtr>), default, gMethod);
                var converted = (Action<IntPtr>)targetDelegate;

                converted.Invoke((IntPtr)wrapperPtr);

                _state.ExecuteSystems.Add(new ExecuteSystemData { Wrapper = wrapperPtr });
            }

            state.SystemsHash.Add(SystemsTools.GetId<T>());
        }

        private static void AddAwake<T>(T instance) where T : unmanaged, IAwake => instance.OnAwake();

        private static void AddExecute<T>(IntPtr ptr) where T : unmanaged, IExecuteSystem
        {
            var wrapperPtr = (ExecuteSystemWrapper*)ptr;
            wrapperPtr->MethodPtr = SystemsToolsExecute<T>.MakeExecuteMethod();
            wrapperPtr->Matcher = (*(T*)ptr).Matcher;
        }

        private unsafe void AddExecute<T>(SystemsState state, IntPtr ptr) where T : unmanaged, IExecuteSystem
        {
            state.ExecuteSystems.Add
            (
                new ExecuteSystemData { Wrapper = (ExecuteSystemWrapper*)ptr }
            );
        }

        private bool Has<T>() where T : ISystem
        {
            var id = SystemsTools.GetId<T>();
            return _state.SystemsHash.Contains(id);
        }

        public unsafe void Tick(float delta)
        {
            var executeSystems = _state.ExecuteSystems;

            for (var i = 0; i < executeSystems.Count; i++)
            {
                var systemData = executeSystems[i];

                var wrapper = systemData.Wrapper;
                var functionPointer = new FunctionPointer<SystemsTools.Execute>((IntPtr)wrapper->MethodPtr);

                var group = _groupsManager->GetGroup(systemData.Wrapper->Matcher);
                for (var j = 0; j < group.Entities.Count; j++)
                {
                    var entityId = group.Entities[j];
                    functionPointer.Invoke(wrapper, _entitiesManager->GetEntityById(entityId), delta);
                }
            }
        }

        public void PhysicTick(float delta)
        {
            // for (var i = 0; i < _physicsSystems.Count; i++)
            // {
            //     var system = _physicsSystems[i];
            //     var group = _systemGroups[system];
            //
            //     for (var j = 0; j < group.Entities.Count; j++)
            //     {
            //         system.Execute(_entitiesManager.GetEntityById(group.Entities[j]), delta);
            //     }
            // }
        }

        public void Dispose()
        {
            _state.Dispose();
        }
    }
}