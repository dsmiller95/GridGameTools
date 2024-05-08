using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Profiling;

public record DungeonEntityStore : ICachingEntityStore
{
    private IReadOnlyDictionary<EntityId, IDungeonEntity> _entities;
    private ILookup<Vector3Int, EntityId> _mobileEntitiesInTiles;
    private ILookup<Vector3Int, EntityId> _staticEntitiesInTiles;

    public DungeonEntityStore(IReadOnlyDictionary<EntityId, IDungeonEntity> entities)
    {
        _entities = entities;
        
        _staticEntitiesInTiles = entities
            .Where(e => e.Value.IsStatic)
            .Select(e => (e.Value.Coordinate.Position, e.Key))
            .ToLookup(e => e.Position, e => e.Key);
        _mobileEntitiesInTiles = entities
            .Where(e => !e.Value.IsStatic)
            .Select(e => (e.Value.Coordinate.Position, e.Key))
            .ToLookup(e => e.Position, e => e.Key);
    }
    
    
    private DungeonEntityStore(
        IReadOnlyDictionary<EntityId, IDungeonEntity> entities,
        ILookup<Vector3Int, EntityId> mobileEntitiesInTiles,
        ILookup<Vector3Int, EntityId> staticEntitiesInTiles
        )
    {
        _entities = entities;
        _mobileEntitiesInTiles = mobileEntitiesInTiles;
        _staticEntitiesInTiles = staticEntitiesInTiles;
    }
    
    public IEnumerable<(EntityId id, IDungeonEntity entity)> AllEntitiesWithIds()
    {
        return _entities.Select(e => (e.Key, e.Value));
    }

    public IDungeonEntity GetEntity(EntityId id)
    {
        return _entities.GetValueOrDefault(id);
    }

    public IEnumerable<EntityId> GetEntitiesAt(Vector3Int position)
    {
        return _mobileEntitiesInTiles[position].Concat(_staticEntitiesInTiles[position]);
    }

    public IWritableEntities CreateWriter()
    {
        return new WritableDungeonEntities(this);
    }
    
    private class WritableDungeonEntities : IWritableEntities
    {
        private Dictionary<EntityId, IDungeonEntity> modifiedEntities;
        private DictionaryBackedLookup<Vector3Int, EntityId> moblieEntityPositions;
        private DungeonEntityStore underlying;
        private List<EntityWriteRecord> writeOperations;
        
        /// <summary>
        /// COW style structure. null if no changes. If any changes, is a copy of the underlying lookup with changes applied.
        /// </summary>
        [CanBeNull] private DictionaryBackedLookup<Vector3Int, EntityId> newStaticEntities = null;
        
        public WritableDungeonEntities(DungeonEntityStore underlying)
        {
            this.underlying = underlying;
            modifiedEntities = new Dictionary<EntityId, IDungeonEntity>();
            this.moblieEntityPositions = new DictionaryBackedLookup<Vector3Int, EntityId>(this.underlying._mobileEntitiesInTiles);
            writeOperations = new List<EntityWriteRecord>();
            newStaticEntities = null;
        }

        public ICachingEntityStore Build()
        {
            Profiler.BeginSample("WritableDungeonEntities.Build");
            var result = this.BuildInternal();
            Profiler.EndSample();
            return result;
        }

        private DungeonEntityStore BuildInternal()
        {
            var newEntities = new Dictionary<EntityId, IDungeonEntity>(underlying._entities);
            foreach (var modifiedEntity in modifiedEntities)
            {
                if (modifiedEntity.Value == null)
                {
                    newEntities.Remove(modifiedEntity.Key);
                }
                else
                {
                    newEntities[modifiedEntity.Key] = modifiedEntity.Value;
                }
            }

            var staticEntities = newStaticEntities ?? underlying._staticEntitiesInTiles;

            return new DungeonEntityStore(newEntities, this.moblieEntityPositions, staticEntities);
        }

        public IDungeonEntity GetEntity(EntityId id)
        {
            if (modifiedEntities.TryGetValue(id, out IDungeonEntity entity))
            {
                return entity;
            }
            return underlying.GetEntity(id);
        }

        public IEnumerable<EntityId> GetEntitiesAt(Vector3Int position)
        {
            var staticEntities = newStaticEntities ?? underlying._staticEntitiesInTiles;
            return moblieEntityPositions[position].Concat(staticEntities[position]);
        }

        public IEnumerable<EntityWriteRecord> WriteOperations()
        {
            return writeOperations;
        }

        public EntityWriteRecord SetEntity(EntityId id, IDungeonEntity newValue)
        {
            var oldValue = this.GetEntity(id);
            if (oldValue == newValue) return null;

            if (oldValue != newValue)
            {
                if (oldValue != null)
                {
                    this.RemoveEntityPosition(oldValue, id);
                }
                this.AddEntityPosition(newValue, id);
            }
            
            modifiedEntities[id] = newValue;
            var writeType = oldValue == null ? EntityWriteRecord.Change.Add : EntityWriteRecord.Change.Update;
            var writeRecord = new EntityWriteRecord(id, oldValue, newValue, writeType);
            writeOperations.Add(writeRecord);
            return writeRecord;
        }

        public EntityWriteRecord CreateEntity(IDungeonEntity entity)
        {
            var id = EntityId.New();
            modifiedEntities[id] = entity;
            AddEntityPosition(entity, id);
            var writeOperation = new EntityWriteRecord(id, null, entity, EntityWriteRecord.Change.Add);
            writeOperations.Add(writeOperation);
            return writeOperation;
        }

        public EntityWriteRecord RemoveEntity(EntityId id)
        {
            var oldValue = this.GetEntity(id);
            if (oldValue == null) return null; // already removed, or not present
            if (!RemoveEntityPosition(oldValue, id))
            {
                throw new InvalidOperationException($"Entity not in position map {oldValue}");
            }
            modifiedEntities[id] = null;
            var writeOperation = new EntityWriteRecord(id, oldValue, null, EntityWriteRecord.Change.Delete); 
            writeOperations.Add(writeOperation);
            return writeOperation;
        }
        
        private DictionaryBackedLookup<Vector3Int, EntityId> EnsureCopiedStaticEntities()
        {
            if(newStaticEntities != null) return newStaticEntities;
            Debug.LogWarning("Copying static entities. Should only occur once or twice.");
            newStaticEntities = new DictionaryBackedLookup<Vector3Int, EntityId>(underlying._staticEntitiesInTiles);
            return newStaticEntities;
        }
        
        private void AddEntityPosition(IDungeonEntity newEntity, EntityId id)
        {
            if (newEntity.IsStatic)
            {
                EnsureCopiedStaticEntities().Add(newEntity.Coordinate.Position, id);
            }
            else
            {
                this.moblieEntityPositions.Add(newEntity.Coordinate.Position, id);
            }
        }
        
        private bool RemoveEntityPosition(IDungeonEntity oldEntity, EntityId id)
        {
            if (oldEntity.IsStatic)
            {
                return EnsureCopiedStaticEntities().Remove(oldEntity.Coordinate.Position, id);
            }
            else
            {
                return this.moblieEntityPositions.Remove(oldEntity.Coordinate.Position, id);
            }
        }
        
        public IEnumerable<(EntityId id, IDungeonEntity entity)> AllEntitiesWithIds()
        {
            Profiler.BeginSample("WritableDungeonEntities.AllEntitiesWithIds");
            var result = this.BuildInternal().AllEntitiesWithIds();
            Profiler.EndSample();
            return result;
        }

        public IWritableEntities CreateWriter()
        {
            Debug.LogWarning("created a writer to write to an existing writer");
            return this.Build().CreateWriter();
        }
    }

    private readonly Dictionary<Type, (EntityId id, IDungeonEntity entity)?> _memoizedSinglesByType = new(); 
    public (EntityId id, IDungeonEntity entity)? GetSingleOfType<T>()
    {
        if (!_memoizedSinglesByType.TryGetValue(typeof(T), out var result))
        {
            result = AllEntitiesWithIds().SingleOrDefault(e => e.entity is T);
            _memoizedSinglesByType[typeof(T)] = result;
        }

        return result;
    }
}
