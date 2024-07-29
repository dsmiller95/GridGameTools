using System;
using UnityEngine;

public interface IBoundToEntity
{
    public Type BoundType { get; }
    public EntityId EntityId { get; }
    public IDungeonEntity GetEntityObjectGuessGeneric(IDungeonToWorldContext context, Transform componentGenerators);
    public bool TryBind(EntityId entity, IDungeonUpdater updater, IDungeonToWorldContext worldContext, IDungeonWorld world);
}