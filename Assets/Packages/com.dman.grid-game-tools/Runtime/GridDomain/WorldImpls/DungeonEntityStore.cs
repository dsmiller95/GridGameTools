using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Dman.GridGameTools.DataStructures;
using Dman.GridGameTools.Entities;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Profiling;

namespace Dman.GridGameTools
{
    public record DungeonEntityStore : ICachingEntityStore
    {
        /// <summary>
        /// All entities. The entity value must not be null.
        /// </summary>
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
    
        private class WritableDungeonEntities :
#if DUNGEON_SAFETY_CHECKS
        IWritableEntitiesWithWriteRecord
#else
            IWritableEntities
#endif
        {
            /// <summary>
            /// contains all modifications to entities.
            /// </summary>
            /// <remarks>
            /// if the item is null, then the entity has been removed.
            /// Otherwise, it has been replaced/changed
            /// </remarks>
            [ItemCanBeNull] private Dictionary<EntityId, IDungeonEntity> modifiedEntities;
            /// <summary>
            /// stores the position of all mobile entities, for positional lookups
            /// </summary>
            private DictionaryBackedLookup<Vector3Int, EntityId> moblieEntityPositions;
            private DungeonEntityStore underlying;
            /// <summary>
            /// The entity IDs that have been added at any point during the lifetime of this writer. <br/>
            /// Entries in this list are guaranteed to not be in the underlying store's list of entities.
            /// </summary>
            /// <remarks>
            /// used to accelerate queries. To iterate all entities, we may iterate the underlying store's list
            /// and this list.
            /// </remarks>
            private List<EntityId> addedEntities;

            private bool _isDisposed = false;

#if DUNGEON_SAFETY_CHECKS
        private List<EntityWriteRecord> writeOperations;
#endif
        
            /// <summary>
            /// COW style structure. null if no changes. If any changes, is a copy of the underlying lookup with changes applied.
            /// </summary>
            [CanBeNull] private DictionaryBackedLookup<Vector3Int, EntityId> newStaticEntities = null;
        
            public WritableDungeonEntities(DungeonEntityStore underlying)
            {
                this.underlying = underlying;
                modifiedEntities = new Dictionary<EntityId, IDungeonEntity>();
                this.moblieEntityPositions = new DictionaryBackedLookup<Vector3Int, EntityId>(this.underlying._mobileEntitiesInTiles);
#if DUNGEON_SAFETY_CHECKS
            writeOperations = new List<EntityWriteRecord>();
#endif
                newStaticEntities = null;
                addedEntities = new List<EntityId>(10);// Pools.EntityIdLists.Rent();
            }

            public ICachingEntityStore Build(bool andDispose)
            {
                if(_isDisposed) throw new ObjectDisposedException("WritableDungeonEntities");
                
                Profiler.BeginSample("WritableDungeonEntities.Build");
                var result = this.BuildInternal(andDispose);
                Profiler.EndSample();
                return result;
            }

            private DungeonEntityStore BuildInternal(bool andDispose)
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

                if (andDispose)
                {
                    this.Dispose();
                }

                return new DungeonEntityStore(newEntities, this.moblieEntityPositions, staticEntities);
            }

            public IDungeonEntity GetEntity(EntityId id)
            {
                if(_isDisposed) throw new ObjectDisposedException("WritableDungeonEntities");
                if (modifiedEntities.TryGetValue(id, out IDungeonEntity entity))
                {
                    return entity;
                }
                return underlying.GetEntity(id);
            }

            public IEnumerable<EntityId> GetEntitiesAt(Vector3Int position)
            {
                if(_isDisposed) throw new ObjectDisposedException("WritableDungeonEntities");
                var staticEntities = newStaticEntities ?? underlying._staticEntitiesInTiles;
                return moblieEntityPositions[position].Concat(staticEntities[position]);
            }

            public EntityWriteRecord SetEntity(EntityId id, IDungeonEntity newValue)
            {
                if(newValue == null) throw new ArgumentNullException(nameof(newValue));
                if(_isDisposed) throw new ObjectDisposedException("WritableDungeonEntities");
                
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
                AddWriteRecord(writeRecord);
                if(writeType == EntityWriteRecord.Change.Add) addedEntities.Add(id);
                return writeRecord;
            }

            public EntityWriteRecord CreateEntity(IDungeonEntity entity)
            {
                if(entity == null) throw new ArgumentNullException(nameof(entity));
                if(_isDisposed) throw new ObjectDisposedException("WritableDungeonEntities");
                
                var id = EntityId.New();
                modifiedEntities[id] = entity;
                AddEntityPosition(entity, id);
                var writeOperation = new EntityWriteRecord(id, null, entity, EntityWriteRecord.Change.Add);
                AddWriteRecord(writeOperation);
                addedEntities.Add(id);
                return writeOperation;
            }

            public EntityWriteRecord RemoveEntity(EntityId id)
            {
                if(_isDisposed) throw new ObjectDisposedException("WritableDungeonEntities");
                var oldValue = this.GetEntity(id);
                if (oldValue == null) return null; // already removed, or not present
                if (!RemoveEntityPosition(oldValue, id))
                {
                    throw new InvalidOperationException($"Entity not in position map {oldValue}");
                }
                modifiedEntities[id] = null;
                var writeOperation = new EntityWriteRecord(id, oldValue, null, EntityWriteRecord.Change.Delete); 
                AddWriteRecord(writeOperation);
                return writeOperation;
            }

            public IEnumerable<(EntityId id, IDungeonEntity entity)> AllEntitiesWithIds()
            {
                if(_isDisposed) throw new ObjectDisposedException("WritableDungeonEntities");
                //Profiler.BeginSample("WritableDungeonEntities.AllEntitiesWithIds");
                foreach (var entityId in underlying._entities.Keys)
                {
                    yield return (entityId, GetEntity(entityId));
                }
                foreach (var addedEntity in addedEntities)
                {
                    yield return (addedEntity, GetEntity(addedEntity));
                }
                //var result = this.BuildInternal().AllEntitiesWithIds();
                //Profiler.EndSample();
                //return result;
            }

            public IWritableEntities CreateWriter()
            {
                if(_isDisposed) throw new ObjectDisposedException("WritableDungeonEntities");
                Debug.LogWarning("created a writer to write to an existing writer");
                return this.Build(andDispose: false).CreateWriter();
            }


            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void AddWriteRecord(EntityWriteRecord record)
            {
#if DUNGEON_SAFETY_CHECKS
            writeOperations.Add(record);
#endif
            }

#if DUNGEON_SAFETY_CHECKS
        public IEnumerable<EntityWriteRecord> WriteOperations()
        {
            return writeOperations;
        }
#endif

            private DictionaryBackedLookup<Vector3Int, EntityId> EnsureCopiedStaticEntities()
            {
                if(newStaticEntities != null) return newStaticEntities;
                Debug.Log("Copying static entities. Should only occur once or twice.");
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
        
            
            private void ReleaseUnmanagedResources()
            {
                if (_isDisposed) return;
                _isDisposed = true;
                Pools.EntityIdLists.Return(addedEntities);
            }

            public void Dispose()
            {
                ReleaseUnmanagedResources();
                GC.SuppressFinalize(this);
            }

            ~WritableDungeonEntities()
            {
                ReleaseUnmanagedResources();
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
}
