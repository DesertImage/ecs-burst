using DesertImage.ECS;
using Game.Tween;
using NUnit.Framework;
using Unity.Mathematics;

public class TweenTests
{
    [Test]
    public void Position()
    {
        var world = Worlds.Create();

        const int entitiesCount = 1;
        var entities = new Entity[entitiesCount];
        var results = new float3[entitiesCount];
        var secondResults = new float3[entitiesCount];

        for (var i = 0; i < entitiesCount; i++)
        {
            var entity = world.GetNewEntity();
            entity.Replace<Position>();

            entity.Replace
            (
                new TweenPosition
                {
                    End = new float3(1f),
                    Time = 1f
                }
            );

            entities[i] = entity;
        }

        const ExecutionType executionType = ExecutionType.MultiThread;

        world.Add<TweenPositionCancelSystem>(executionType);
        world.Add<TweenPositionTimeSystem>(executionType);
        world.Add<TweenPositionSystem>(executionType);

        world.Tick(.5f);

        for (var i = 0; i < entitiesCount; i++)
        {
            var entity = entities[i];

            results[i] = entity.Read<Position>().Value;

            entity.Replace<TweenPositionCancel>();
        }

        world.Tick(.25f);

        for (var i = 0; i < entitiesCount; i++)
        {
            secondResults[i] = entities[i].Read<Position>().Value;
        }

        world.Dispose();

        for (var i = 0; i < entitiesCount; i++)
        {
            Assert.AreEqual(.5f, results[i].x);
            Assert.AreEqual(.5f, secondResults[i].x);
        }
    }

    [Test]
    public void Rotation()
    {
        var world = Worlds.Create();

        const int entitiesCount = 1;
        var entities = new Entity[entitiesCount];
        var results = new float3[entitiesCount];
        var secondResults = new float3[entitiesCount];

        for (var i = 0; i < entitiesCount; i++)
        {
            var entity = world.GetNewEntity();
            entity.Replace<Rotation>();

            entity.Replace
            (
                new TweenRotation
                {
                    End = new float3(1f),
                    Time = 1f
                }
            );

            entities[i] = entity;
        }

        const ExecutionType executionType = ExecutionType.MultiThread;

        world.Add<TweenRotationCancelSystem>(executionType);
        world.Add<TweenRotationTimeSystem>(executionType);
        world.Add<TweenRotationSystem>(executionType);

        world.Tick(.5f);

        for (var i = 0; i < entitiesCount; i++)
        {
            var entity = entities[i];

            results[i] = entity.Read<Rotation>().Value;

            entity.Replace<TweenRotationCancel>();
        }

        world.Tick(.25f);

        for (var i = 0; i < entitiesCount; i++)
        {
            secondResults[i] = entities[i].Read<Rotation>().Value;
        }

        world.Dispose();

        for (var i = 0; i < entitiesCount; i++)
        {
            Assert.AreEqual(.5f, results[i].x);
            Assert.AreEqual(.5f, secondResults[i].x);
        }
    }

    [Test]
    public void Scale()
    {
        var world = Worlds.Create();

        const int entitiesCount = 1;
        var entities = new Entity[entitiesCount];
        var results = new float3[entitiesCount];
        var secondResults = new float3[entitiesCount];

        for (var i = 0; i < entitiesCount; i++)
        {
            var entity = world.GetNewEntity();
            entity.Replace<Scale>();

            entity.Replace
            (
                new TweenScale
                {
                    End = new float3(1f),
                    Time = 1f
                }
            );

            entities[i] = entity;
        }

        const ExecutionType executionType = ExecutionType.MultiThread;

        world.Add<TweenScaleCancelSystem>(executionType);
        world.Add<TweenScaleTimeSystem>(executionType);
        world.Add<TweenScaleSystem>(executionType);

        world.Tick(.5f);

        for (var i = 0; i < entitiesCount; i++)
        {
            var entity = entities[i];

            results[i] = entity.Read<Scale>().Value;

            entity.Replace<TweenScaleCancel>();
        }

        world.Tick(.25f);

        for (var i = 0; i < entitiesCount; i++)
        {
            secondResults[i] = entities[i].Read<Scale>().Value;
        }

        world.Dispose();

        for (var i = 0; i < entitiesCount; i++)
        {
            Assert.AreEqual(.5f, results[i].x);
            Assert.AreEqual(.5f, secondResults[i].x);
        }
    }
}