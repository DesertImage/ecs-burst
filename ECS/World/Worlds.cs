namespace DesertImage.ECS
{
    public static class Worlds
    {
        public static World Current;

        private static int _idCounter = -1;

        public static World Create()
        {
            var world = new World(++_idCounter);
            Current = world;
            return world;
        }
    }
}