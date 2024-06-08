using System;

public interface IBoundToEntity
{
    public Type BoundType { get; }
    public EntityId EntityId { get; }
    public IDungeonEntity GetEntityObjectGuessGeneric(IDungeonToWorldContext context);
    public bool TryBind(EntityId entity, IDungeonUpdater updater, IDungeonToWorldContext worldContext, IDungeonWorld world);
}