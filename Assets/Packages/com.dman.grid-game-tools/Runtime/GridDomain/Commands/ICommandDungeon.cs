using Dman.GridGameTools.Entities;
using Dman.GridGameTools.PathingData;
using JetBrains.Annotations;

namespace Dman.GridGameTools.Commands
{
    public interface ICommandDungeon
    {
        public IDungeonEntity GetEntity(EntityId id);
        /// <summary>
        /// Only valid if <paramref name="id"/> already exists in the world
        /// </summary>
        /// <param name="id"></param>
        /// <param name="entity"></param>
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
}