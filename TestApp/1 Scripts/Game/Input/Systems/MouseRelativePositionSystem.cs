using DesertImage.ECS;
using Unity.Mathematics;
using UnityEngine;

namespace Game.Input
{
    public struct MouseRelativePositionSystem : IExecuteSystem
    {
        public void Execute(ref SystemsContext context)
        {
            var world = context.World;

            var mousePosition = world.ReadStatic<MousePosition>();
            var mouseRelativePosition = world.ReadStatic<MouseRelativePosition>();

            var position = mousePosition.Value;

            var width = Screen.width;
            var height = Screen.height;

            mousePosition.Value = position;
            mouseRelativePosition.Value = new float3(position.x / width, position.y / height, 0f);

            world.ReplaceStatic(mouseRelativePosition);
        }
    }
}