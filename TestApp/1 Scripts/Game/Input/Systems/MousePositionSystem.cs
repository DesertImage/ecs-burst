using DesertImage.ECS;

namespace Game.Input
{
    public struct MousePositionSystem : IExecuteSystem
    {
        public void Execute(ref SystemsContext context)
        {
            var world = context.World;

            var mousePosition = world.ReadStatic<MousePosition>();
            var position = UnityEngine.Input.mousePosition;
            
            mousePosition.Value = position;

            world.ReplaceStatic(mousePosition);
        }
    }
}