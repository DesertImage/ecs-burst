namespace DesertImage.ECS
{
    public enum ExecutionOrder
    {
        EarlyMainThread,
        MultiThread,
        LateMainThread,
        RemoveTags,
        Physics
    }
}