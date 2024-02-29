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
            public EntitiesGroup Group;
            public FunctionPointer<SystemsTools.Execute> Method;
            public float DeltaTime;

            [NativeDisableUnsafePtrRestriction] public ExecuteSystemWrapper* Wrapper;
            [NativeDisableUnsafePtrRestriction] public World* World;

            public void Execute(int index) => Method.Invoke(Wrapper, Group.GetEntityId(index), World, DeltaTime);

            public void Execute()
            {
                for (var i = 0; i < Group.Count; i++)
                {
                    Method.Invoke(Wrapper, Group.GetEntityId(i), World, DeltaTime);
                }
            }
        }

        public static void Add<T>(in World world, ExecutionType type) where T : unmanaged, ISystem
        {
            var systemType = typeof(T);
            var systemId = SystemsTools.GetId<T>();

            var state = world.SystemsState;

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

                var targetDelegate = Delegate.CreateDelegate
                (
                    typeof(Action<World, IntPtr, ExecutionType>),
                    default,
                    gMethod
                );

                var converted = (Action<World, IntPtr, ExecutionType>)targetDelegate;

                converted.Invoke(world, (IntPtr)wrapperPtr, type);
            }

            state->SystemsHash.Set(systemId, systemId);
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

            state->SystemsHash.Remove(systemId);

            for (var i = 0; i < state->EarlyMainThreadSystems.Count; i++)
            {
                var systemData = state->EarlyMainThreadSystems[i];
                if (systemData.Id != systemId) continue;
                state->EarlyMainThreadSystems.RemoveAt(i);
                return;
            }

            for (var i = 0; i < state->LateMainThreadSystems.Count; i++)
            {
                var systemData = state->LateMainThreadSystems[i];
                if (systemData.Id != systemId) continue;
                state->LateMainThreadSystems.RemoveAt(i);
                return;
            }

            for (var i = 0; i < state->MultiThreadSystems.Count; i++)
            {
                var systemData = state->MultiThreadSystems[i];
                if (systemData.Id != systemId) continue;
                state->MultiThreadSystems.RemoveAt(i);
                break;
            }
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

                var group = Groups.GetSystemGroup(systemData.Id, *world);

                for (var j = 0; j < group.Count; j++)
                {
                    var entityId = group.GetEntityId(j);
                    functionPointer.Invoke(wrapper, entityId, world, deltaTime);
                }
            }
        }

        public static void ExecuteMultiThread(ref UnsafeList<ExecuteSystemData> systems, World* world, float deltaTime)
        {
            var systemsState = world->SystemsState;

            for (var i = 0; i < systems.Count; i++)
            {
                var systemData = systems[i];

                var wrapper = systemData.Wrapper;
                var group = Groups.GetSystemGroup(systemData.Id, *world);

                var executeJob = new ExecuteSystemJob
                {
                    Group = group,
                    Wrapper = wrapper,
                    Method = new FunctionPointer<SystemsTools.Execute>((IntPtr)wrapper->MethodPtr),
                    World = world,
                    DeltaTime = deltaTime
                };

                // systemsState->Handle = executeJob.Schedule(group.Count, 128, systemsState->Handle);
                systemsState->Handle = executeJob.Schedule(systemsState->Handle);
            }

            systemsState->Handle.Complete();
        }

        private static void AddInit<T>(T instance) where T : unmanaged, IInitSystem => instance.Initialize();

        private static void AddExecute<T>(World world, IntPtr wrapperPtr, ExecutionType type)
            where T : unmanaged, IExecuteSystem
        {
            var systemId = SystemsTools.GetId<T>();

            var wrapper = (ExecuteSystemWrapper*)wrapperPtr;
            wrapper->MethodPtr = SystemsToolsExecute<T>.MakeExecuteMethod();

            var matcherId = Groups.GetSystemMatcherId(systemId, world);
            if (matcherId > 0)
            {
                wrapper->MatcherId = matcherId;
            }
            else
            {
                var matcher = (*(T*)wrapperPtr).Matcher;

                Groups.RegisterSystemMatcher(systemId, matcher, world);

                wrapper->MatcherId = matcher.Id;
            }

            var state = world.SystemsState;

            var data = new ExecuteSystemData
            {
                Id = systemId,
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