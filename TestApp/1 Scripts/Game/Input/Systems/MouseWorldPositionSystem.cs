using DesertImage.ECS;
using Camera = Game.Cameras.Camera;

namespace Game.Input
{
    public struct MouseWorldPositionSystem : IExecuteSystem
    {
        public void Execute(ref SystemsContext context)
        {
            var world = context.World;

            var mousePosition = world.ReadStatic<MousePosition>();
            var mouseWorldPosition = world.ReadStatic<MouseWorldPosition>();
            var camera = world.ReadStatic<Camera>().Value.Value;
            
            var position = mousePosition.Value;
            position.z = mouseWorldPosition.ZOffset;
            
            mouseWorldPosition.Value = camera.ScreenToWorldPoint(position);

            world.ReplaceStatic(mouseWorldPosition);
        }
    }
}