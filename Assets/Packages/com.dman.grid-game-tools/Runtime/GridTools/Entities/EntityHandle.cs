public record EntityHandle<T> where T: IDungeonEntity
{
    public readonly EntityId Id;
    internal EntityHandle(EntityId id)
    {
        Id = id;
    }

    public static implicit operator EntityId(EntityHandle<T> typedHandle)
    {
        return typedHandle.Id;
    }
}