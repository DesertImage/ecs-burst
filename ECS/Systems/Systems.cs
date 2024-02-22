using System;
using System.Reflection;
using DesertImage.Collections;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;

namespace DesertImage.ECS
{
    public static unsafe class Systems
    {
        [BurstCompile]
        private struct ExecuteSystemJob : IJob
        {
            public ExecuteSystemData SystemData;
            [NativeDisableUnsafePtrRestriction] public World* World;

            public void Execute()
            {
                var wrapper = SystemData.Wrapper;
                var functionPointer = new FunctionPointer<SystemsTools.Execute>((IntPtr)wrapper->MethodPtr);

                var group = Groups.GetGroup(SystemData.Wrapper->Matcher, *World);
                foreach (var entityId in group.Entities)
                {
                    functionPointer.Invoke(wrapper, entityId, World, World->SystemsState->DeltaTime);
                }
            }
        }

        public static void Add<T>(SystemsState* state, ExecutionType type) where T : unmanaged, ISystem
        {
            var systemType = typeof(T);

            if (Contains<T>(state))
            {
#if DEBUG
                throw new Exception($"system {systemType} already added");
#endif
                return;
            }

            var isInit = typeof(IInitSystem).IsAssignableFrom(systemType);
            if (isInit)
            {
                var methodInfo = typeof(Systems).GetMethod
                (
                    nameof(AddInit),
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
                    Value = MemoryUtility.Allocate(default(T)),
                };

                var wrapperPtr = MemoryUtility.Allocate(wrapper);

                var methodInfo = typeof(Systems).GetMethod
                (
                    nameof(AddExecute),
                    BindingFlags.Static | BindingFlags.NonPublic
                );

                var gMethod = methodInfo.MakeGenericMethod(systemType);

                var targetDelegate =
                    Delegate.CreateDelegate(typeof(Action<IntPtr, IntPtr, ExecutionType>), default, gMethod);
                var converted = (Action<IntPtr, IntPtr, ExecutionType>)targetDelegate;

                converted.Invoke((IntPtr)state, (IntPtr)wrapperPtr, type);
            }

            state->SystemsHash.Add(SystemsTools.GetId<T>());
        }

        public static void Remove<T>(SystemsState* state) where T : ISystem
        {
            if (!Contains<T>(state))
            {
#if DEBUG
                throw new Exception($"system {typeof(T)} haven't been added");
#endif
                return;
            }

            var systemId = SystemsTools.GetId<T>();

            for (var i = 0; i < state->MultiThreadSystems.Count; i++)
            {
                var systemData = state->MultiThreadSystems[i];

                if (systemData.Id != systemId) continue;

                state->MultiThreadSystems.RemoveAt(i);
                break;
            }

            state->SystemsHash.Remove(systemId);
        }

        public static bool Contains<T>(SystemsState* state) where T : ISystem
        {
            var id = SystemsTools.GetId<T>();
            return state->SystemsHash.Contains(id);
        }

        public static void Execute(World* world, float deltaTime)
        {
            ExecuteMainThread(ref world->SystemsState->EarlyMainThreadSystems, world, deltaTime);
            ExecuteMultiThread(ref world->SystemsState->MultiThreadSystems, world, deltaTime);
            ExecuteMainThread(ref world->SystemsState->LateMainThreadSystems, world, deltaTime);

        }

        private static void ExecuteMainThread(ref UnsafeList<ExecuteSystemData> systems, World* world, float deltaTime)
        {
            for (var i = 0; i < systems.Count; i++)
            {
                var systemData = systems[i];

                var wrapper = systemData.Wrapper;
                var functionPointer = new FunctionPointer<SystemsTools.Execute>((IntPtr)wrapper->MethodPtr);

                var group = Groups.GetGroup(systemData.Wrapper->Matcher, *world);
                foreach (var entityId in group.Entities)
                {
                    functionPointer.Invoke(wrapper, entityId, world, deltaTime);
                }
            }
        }
        
        public static void ExecuteMultiThread(ref UnsafeList<ExecuteSystemData> systems, World* world, float deltaTime)
        {
            var systemsState = world->SystemsState;

            systemsState->DeltaTime = deltaTime;

            for (var i = 0; i < systems.Count; i++)
            {
                var systemData = systems[i];

                var executeJob = new ExecuteSystemJob
                {
                    SystemData = systemData,
                    World = world
                };

                systemsState->Handle = executeJob.Schedule(systemsState->Handle);
            }

            systemsState->Handle.Complete();
        }

        private static void AddInit<T>(T instance) where T : unmanaged, IInitSystem => instance.Initialize();

        private static void AddExecute<T>(IntPtr statePtr, IntPtr wrapperPtr, ExecutionType type)
            where T : unmanaged, IExecuteSystem
        {
            var wrapper = (ExecuteSystemWrapper*)wrapperPtr;
            wrapper->MethodPtr = SystemsToolsExecute<T>.MakeExecuteMethod();
            wrapper->Matcher = (*(T*)wrapperPtr).Matcher;

            var state = (SystemsState*)statePtr;

            var data = new ExecuteSystemData
            {
                Id = SystemsTools.GetId<T>(),
                Wrapper = wrapper
            };

            switch (type)
            {
                case ExecutionType.EarlyMainThread:
                    state->EarlyMainThreadSystems.Add(data);
                    break;
                case ExecutionType.LateMainThread:
                    state->LateMainThreadSystems.Add(data);
                    break;
                default:
                    state->MultiThreadSystems.Add(data);
                    break;
            }
        }
    }
}