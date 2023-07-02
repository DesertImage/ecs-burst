using System.Collections.Generic;

namespace DesertImage.ECS
{
    public static class ComponentsDebug
    {
#if UNITY_EDITOR
        public static Dictionary<int, object[]> Components = new Dictionary<int, object[]>();
#endif
    }
}