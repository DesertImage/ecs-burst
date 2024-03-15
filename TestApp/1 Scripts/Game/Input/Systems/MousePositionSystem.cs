using DesertImage.ECS;
using Camera = Game.Cameras.Camera;

namespace Game.Input
{
    public struct MousePositionSystem : IExecuteSystem
    {
        public void Execute(ref SystemsContext context)
        {
            var world = context.World;

            var mousePosition = world.GetStatic<MousePosition>();
            var camera = world.GetStatic<Camera>().Value.Value;
            var position = UnityEngine.Input.mousePosition;
            position.z = mousePosition.ZOffset;
            
            mousePosition.Value = position;
            mousePosition.WorldPosition = camera.ScreenToWorldPoint(position);

            world.ReplaceStatic(mousePosition);
        }
    }
}