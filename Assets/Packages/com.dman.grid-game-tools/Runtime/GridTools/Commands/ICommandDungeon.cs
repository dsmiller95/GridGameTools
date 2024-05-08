
using JetBrains.Annotations;

public interface ICommandDungeon
{
    public IDungeonEntity GetEntity(EntityId id);
    public void SetEntity(EntityId id, [CanBeNull] IDungeonEntity entity);
    public EntityId CreateEntity([NotNull] IDungeonEntity entity);
    public IDungeonPathingData CurrentPathingState { get; }
    public IEntityStore CurrentEntityState { get; }
    public IDungeonWorld PreviousWorldState { get; }
}