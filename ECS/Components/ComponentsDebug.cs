using System.Collections.Generic;

namespace DesertImage.ECS
{
    public static class ComponentsDebug
    {
#if UNITY_EDITOR
        public static Dictionary<uint, object[]> Components = new Dictionary<uint, object[]>();
#endif
    }
}