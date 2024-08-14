public record EntityId
{
    private readonly int _value;
    public static readonly EntityId Invalid = new EntityId(-1);
    private static int lastId = 0;
    
    private EntityId(int value)
    {
        _value = value;
    }
    public EntityId(EntityId previous)
    {
        _value = previous._value + 1;
    }
    
    public static EntityId New()
    {
        return new EntityId(++lastId);
    }

    public override string ToString()
    {
        return "id:" + _value;
    }
}
public static class EntityIdExtensions
{
    public static bool IsValid(this EntityId id)
    {
        return id != null && id != EntityId.Invalid;
    }
}