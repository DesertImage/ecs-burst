namespace DesertImage.ECS
{
    public unsafe struct ExecuteSystemWrapper
    {
        public void* Value;
        public Matcher Matcher;
        public void* MethodPtr;
    }
}