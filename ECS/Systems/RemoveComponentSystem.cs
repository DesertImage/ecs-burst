﻿namespace DesertImage.ECS
{
    public struct RemoveComponentSystem<T> : IExecuteSystem where T : unmanaged
    {
        public Matcher Matcher => MatcherBuilder.Create().With<T>().Build();

        public void Execute(Entity entity, World world, float deltaTime) => entity.Remove<T>();
    }
}