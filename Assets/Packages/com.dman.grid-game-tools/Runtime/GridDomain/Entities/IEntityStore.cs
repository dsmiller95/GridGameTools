using System;
using System.Collections.Generic;
using System.Linq;
using Dman.GridGameTools.Commands;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Profiling;

namespace Dman.GridGameTools.Entities
{
    public interface ICachingEntityStore : IEntityStoreCachedOperations, IEntityStore
    {
    }

    public interface IEntityStoreCachedOperations
    {
        public (EntityId id, IDungeonEntity entity)? GetSingleOfType<T>();
    }

    public interface IEntityStore
    {
        public IEnumerable<(EntityId id, IDungeonEntity entity)> AllEntitiesWithIds();
        [CanBeNull] public IDungeonEntity GetEntity([NotNull] EntityId id);
        IEnumerable<EntityId> GetEntitiesAt(Vector3Int position);
    
        public IWritableEntities CreateWriter();

        public T GetEntityAssert<T>([NotNull] EntityId id)
        {
            var entity = GetEntity(id);
            if (entity is not T typedEntity) throw new Exception($"Entity {id} is not of type {typeof(T)}");
            return typedEntity;
        }
        public IDungeonEntity GetEntityAssert([NotNull] EntityId id)
        {
            var entity = GetEntity(id);
            if (entity is not { } typedEntity) throw new Exception($"Entity {id} does not exist");
            return typedEntity;
        }
    
        public IEnumerable<EntityId> AllEntities()
        {
            return AllEntitiesWithIds().Select(x => x.id);
        }
    
        public IEnumerable<(EntityId id, IDungeonEntity entity)> GetEntityPairsAt(Vector3Int position)
        {
            foreach (var entityId in GetEntitiesAt(position))
            {
                var entityObject = GetEntity(entityId);
                yield return (entityId,  entityObject);
            }
        }
        public IEnumerable<EntityId> GetEntitiesOfTypeAt<T>(Vector3Int position)
        {
            foreach (var entityId in GetEntitiesAt(position))
            {
                var entityObject = GetEntity(entityId);
                if (entityObject is not T typedEntity) continue;
                yield return entityId;
            }
        }
        public IEnumerable<EntityId> GetEntitiesOfType<T>()
        {
            return AllEntitiesWithIds()
                .Where(e => e.entity is T)
                .Select(e => e.id);
        }
        public IEnumerable<T> GetEntityObjectsOfType<T>() where T : class
        {
            return AllEntitiesWithIds()
                .Select(e => e.entity as T)
                .Where(e => e != null);
        }

        public IEnumerable<IDungeonEntity> GetEntityObjectsAt(Vector3Int position)
        {
            foreach (var entityId in GetEntitiesAt(position))
            {
                yield return GetEntity(entityId);
            }
        }
        public IEnumerable<(EntityId, T)> GetEntityPairsOfTypeAt<T>(Vector3Int position) where T: class
        {
            foreach (var entityId in GetEntitiesAt(position))
            {
                var entityObject = GetEntity(entityId);
                if (entityObject is not T typedEntity) continue;
                yield return (entityId, typedEntity);
            }
        }
        public IEnumerable<T> GetEntityObjectsOfTypeAt<T>(Vector3Int position) where T: class
        {
            foreach (var entityId in GetEntitiesAt(position))
            {
                var entityObject = GetEntity(entityId);
                if (entityObject is not T typedEntity) continue;
                yield return typedEntity;
            }
        }
    
        public T GetEntity<T>(EntityHandle<T> handle) where T : IDungeonEntity
        {
            return (T) GetEntity(handle.Id);
        }
        [CanBeNull]
        public EntityHandle<T> TryGetHandle<T>(EntityId id) where T : IDungeonEntity
        {
            var entity = this.GetEntity(id);
            if (entity is T)
            {
                return new EntityHandle<T>(id);
            }

            return null;
        }

        public IDungeonCommand ModifyCommandUntilSettled(IDungeonCommand command)
        {
            Profiler.BeginSample("ModifyCommandUntilSettled");
            var ogCommand = command;
        
            var didMakeChange = true;
            var infiniteProtection = 1000;
            var thingsThatHaveModified = new HashSet<object>();
            bool HasModifiedSomethingAlready(object modifier) => thingsThatHaveModified.Contains(modifier);
            while (didMakeChange && (--infiniteProtection) > 0)
            {
                didMakeChange = false;
                if (command.ActionTaker == null)
                {
                    continue;
                }
            
                var commandTaker = this.GetEntity(command.ActionTaker);
            
                if (commandTaker is not IModifyOwnCommands modifier || HasModifiedSomethingAlready(commandTaker))
                {
                    continue;
                }

                var (modifiedBy, modified) = modifier.ModifyCommand(command, HasModifiedSomethingAlready);
                if (modified == command) continue;
            
                if (modifiedBy != null) thingsThatHaveModified.Add(modifiedBy);
                didMakeChange = true;
                command = modified;
            }
            Profiler.EndSample();

            if (didMakeChange)
            {
                throw new Exception("Commands never settled");
            }

            return command;
        }
    }

    public interface IWritableEntities : IEntityStore, IDisposable
    {
        public ICachingEntityStore Build(bool andDispose);
        [CanBeNull] public EntityWriteRecord SetEntity(EntityId id, IDungeonEntity entity);
        [NotNull] public EntityWriteRecord CreateEntity(IDungeonEntity entity);
        [CanBeNull] public EntityWriteRecord RemoveEntity(EntityId id);
    
    }

#if DUNGEON_SAFETY_CHECKS
/// <summary>
/// only useful when DUNGEON_SAFETY_CHECKS is defined
/// </summary>
internal interface IWritableEntitiesWithWriteRecord : IWritableEntities
{
    public IEnumerable<EntityWriteRecord> WriteOperations();
}
#endif

    public record EntityWriteRecord(EntityId Id, [CanBeNull] IDungeonEntity OldEntity, [CanBeNull] IDungeonEntity NewEntity, EntityWriteRecord.Change ChangeType)
    {
        public EntityId Id { get; } = Id;
        [CanBeNull] public IDungeonEntity OldEntity { get; } = OldEntity;
        [CanBeNull] public IDungeonEntity NewEntity { get; } = NewEntity;
        public Change ChangeType { get; } = ChangeType;

        public enum Change
        {
            Add,
            Delete,
            Update
        }
    }
}