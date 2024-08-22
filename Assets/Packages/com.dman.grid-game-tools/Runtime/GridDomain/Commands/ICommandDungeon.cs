
using Dman.GridGameTools;
using JetBrains.Annotations;

public interface ICommandDungeon
{
    public IDungeonEntity GetEntity(EntityId id);
    public void SetEntity(EntityId id, [CanBeNull] IDungeonEntity entity);
    public EntityId CreateEntity([NotNull] IDungeonEntity entity);
    public IDungeonPathingData CurrentPathingState => this.WritableComponentStore.AssertGet<IDungeonPathingData>();
    public IEntityStore CurrentEntityState { get; }
    public IDungeonWorld PreviousWorldState { get; }
    public IComponentStore WritableComponentStore { get; }
    
    /// <summary>
    /// Very expensive - use sparingly and rarely.
    /// </summary>
    /// <param name="disposeInternals">
    /// Whether to dispose the internals inside the writer. Only set to true if you have exclusive access to the writer.
    /// </param>
    /// <returns></returns>
    public IDungeonWorld BakeToImmutable(bool disposeInternals = false);
}