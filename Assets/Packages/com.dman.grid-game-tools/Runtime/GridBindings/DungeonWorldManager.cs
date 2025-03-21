using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Dman.GridGameTools;
using Dman.GridGameTools.Commands;
using Dman.GridGameTools.DataStructures;
using Dman.GridGameTools.Entities;
using Dman.GridGameTools.EventLog;
using Dman.GridGameTools.Random;
using Dman.Utilities.Logger;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Profiling;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

namespace Dman.GridGameBindings
{
    public static class DungeonWorldManagerSingleton
    {
        private static DungeonWorldManager instance;
    
        public static DungeonWorldManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = Object.FindObjectOfType<DungeonWorldManager>();
                }

                return instance;
            }
        }
    }

    [DefaultExecutionOrder(1000)]
    public class DungeonWorldManager : MonoBehaviour,
        IDungeonUpdater,
        IDungeonToWorldContext,
        IGridSpace
    {
        /// <summary>
        /// The current world state. 
        /// </summary>
        public IDungeonWorld CurrentWorld => _currentWorld.Value;

        private Rc<IDungeonWorld> _currentWorld = Rc.Create<IDungeonWorld>(null);

        private Rc<IDungeonWorld> _lastWorld;
    
        [SerializeField] private InitialDungeonState initialState = new InitialDungeonState();
        [Tooltip("Used when we want to spawn something into the world. it gets spawned as a child of this object.")]
        [SerializeField] private Transform worldParent;
        [SerializeField] public UnityEvent onAllRenderUpdatesComplete;
        
        [FormerlySerializedAs("swapYAndZ")]
        [Tooltip("When set, will swap the Y and Z axes internally and invert the Z axis. matches unity's 2D layout. -z becomes Up, +z becomes Down")]
        [SerializeField] public bool useUnity2DAxises = false;
    
        [Header("Debug")]
        [SerializeField] public bool logTopLevelCommands = false;
        [SerializeField] public bool logUpdateRenderOrders = false;
    
        private SortedDictionary<int, List<IRenderUpdate>> _updateListeners = new ();
        private AsyncFnOnceCell _updateCell;

        private GridRandomGen? _worldRng = null;

        public GridRandomGen SpawningRng
        {
            get
            {
                if (_worldRng == null)
                {
                    _worldRng = new GridRandomGen(initialState.SeedOrRngIfDefault);
                }

                return _worldRng.Value;
            }
        }

        public Transform EntityParent => worldParent;

        
        private Queue<Rc<IDungeonWorld>> _lastWorldBuffer = new Queue<Rc<IDungeonWorld>>();
        private int _lastWorldBufferCapacity = 1;
        public int OldWorldBufferSize => _lastWorldBuffer.Count + (_lastWorld.Value == null ? 0 : 1);
        /// <summary>
        /// how many old world states to keep around. minimum of 1.
        /// </summary>
        public int OldWorldBufferCapacity
        {
            get => _lastWorldBufferCapacity;
            set
            {
                _lastWorldBufferCapacity = Mathf.Min(1, value);
                while (_lastWorldBuffer.Count > _lastWorldBufferCapacity - 1)
                {
                    _lastWorldBuffer.Dequeue().Dispose();
                }
            }
        }

        public void SeedWith(GridRandomGen seed)
        {
            if (_worldRng != null)
            {
                Log.Error($"Seeding rng of the world, but rng has already been set. this may not be deterministic");
            }

            seed = seed.Fork(nameof(DungeonWorldManager).ToSeed());
            _worldRng = seed;
        }
        
        private void Awake()
        {
            _updateCell = new AsyncFnOnceCell(gameObject);
        }

        private void Start()
        {
            if (CurrentWorld == null)
            { // expect the initial world to be set elsewhere, otherwise, default to a basic initialization.
                var firstWorld = DungeonWorldFactory.CreateDungeonWorld(initialState);
                SetInitialWorldState(firstWorld);
            }
        }

        /// <summary>
        /// sets the world state. must be called before Start runs on DungeonWorldManager, otherwise will fail.
        /// </summary>
        /// <param name="world"></param>
        public void TakeInitialWorldState(IDungeonWorld world)
        {
            if(CurrentWorld != null) throw new InvalidOperationException("Cannot set initial world state after Start has run");
            SetInitialWorldState(world);
        }
        private void SetInitialWorldState(IDungeonWorld initialWorld)
        {
            if(CurrentWorld != null) throw new InvalidOperationException("Cannot set initial world state if world already exists");
            var allPostInit = GetComponents<IPostInitDungeonWorld>();
            foreach (IPostInitDungeonWorld postInit in allPostInit)
            {
                initialWorld = postInit.PostInitialize(initialWorld);
            }
            _currentWorld = Rc.Create(initialWorld);
            TriggerWorldUpdated(new DungeonUpdateEvent(null, CurrentWorld));
        }

        public bool CanUpdateWorld()
        {
            return !_updateCell.IsRunning;
        }
    
        public void ApplyCommand(IDungeonCommand command)
        {
            var commands = new []{command};
            LogCommands(commands);
            var (newWorld, modifiedCommands) = CurrentWorld.ApplyCommandsWithModifiedCommands(commands);
            this.UpdateWorld(Rc.Create(newWorld), modifiedCommands);
        }
        
        
        public void ApplyCommands(IEnumerable<IDungeonCommand> allCommands)
        {
            if(!CanUpdateWorld()) throw new InvalidOperationException("Cannot update world while already updating");
            
            if (logTopLevelCommands)
            {
                // avoid multiple enumerations
                allCommands = allCommands.ToList();
                LogCommands(allCommands);
            }
            var (newWorld, modifiedCommands) = CurrentWorld.ApplyCommandsWithModifiedCommands(allCommands);
            this.UpdateWorld(Rc.Create(newWorld), modifiedCommands);
        }
    
        public void ApplyCommandsIfEmittedEvent(IEnumerable<IDungeonCommand> allCommands, Func<IGridEvent, bool> eventPredicate)
        {
            if(!CanUpdateWorld()) throw new InvalidOperationException("Cannot update world while already updating");
            
            var eventLog = CurrentWorld.Components.TryGet<IEventLog>();
            if (eventLog == null) Log.Error("DungeonWorldManager: no event log found world when using ApplyCommandsIfEmittedEvent. Check that the world is initialized with an event log");
            
            var eventCheckpoint = eventLog?.Checkpoint();
            
            if (logTopLevelCommands)
            {
                // avoid multiple enumerations
                allCommands = allCommands.ToList();
                LogCommands(allCommands);
            }
            var (newWorld, modifiedCommands) = CurrentWorld.ApplyCommandsWithModifiedCommands(allCommands);
            
            eventLog = newWorld.Components.TryGet<IEventLog>();
            if (eventLog != null && eventCheckpoint is {} checkpoint)
            {
                var emittedEvents = eventLog.AllEventsSince(checkpoint);
                if (!emittedEvents.Any(eventPredicate))
                {
                    newWorld.Dispose();
                    return;
                }
            }
        
            this.UpdateWorld(Rc.Create(newWorld), modifiedCommands);
        }
        
        [Obsolete("Prefer to use ApplyCommandsIfEmittedEvent, and emit domain events when an entity moves.")]
        public void ApplyCommands(IEnumerable<IDungeonCommand> allCommands, [CanBeNull] EntityId onlyApplyIfMovedPosition)
        {
            if(!CanUpdateWorld()) throw new InvalidOperationException("Cannot update world while already updating");
            var startPosition = GetPositionOfEntity(CurrentWorld.EntityStore, onlyApplyIfMovedPosition);
 
            if (logTopLevelCommands)
            {
                // avoid multiple enumerations
                allCommands = allCommands.ToList();
                LogCommands(allCommands);
            }
            var (newWorld, modifiedCommands) = CurrentWorld.ApplyCommandsWithModifiedCommands(allCommands);
            var nextPosition = GetPositionOfEntity(newWorld.EntityStore, onlyApplyIfMovedPosition);
            if(startPosition.HasValue && startPosition == nextPosition)
            { // if the entity didn't move, dont do anything at all
                newWorld.Dispose();
                return;
            }
        
            this.UpdateWorld(Rc.Create(newWorld), modifiedCommands);
        }
        
    
        private void LogCommands(IEnumerable<IDungeonCommand> commands)
        {
            if (logTopLevelCommands)
            {
                Debug.Log("DungeonWorldManager Top level Commands:\n" + string.Join("\n", commands.Select(c => c.ToString())));
            }
        }

        private static Vector3Int? GetPositionOfEntity(IEntityStore entityStore, [CanBeNull] EntityId optionalEntity)
        {
            if (optionalEntity == null) return null;
            return entityStore.GetEntity(optionalEntity)
                ?.Coordinate.Position;
        }
    
        public void SpawnEntitiesWithCustomHydration(IDungeonEntity[] entities, Action<IDungeonWorld, EntityId[]> rehydrate)
        {
            if(!CanUpdateWorld()) throw new InvalidOperationException("Cannot update world while already updating");
        
            var (newWorld, addedEntities) = CurrentWorld.AddEntities(entities);
            rehydrate(newWorld, addedEntities.ToArray());
            this.UpdateWorld(Rc.Create(newWorld), Array.Empty<IDungeonCommand>());
        }
    
        public void DespawnEntities(params EntityId[] entities)
        {
            if(!CanUpdateWorld()) throw new InvalidOperationException("Cannot update world while already updating");
        
            var newWorld = CurrentWorld.RemoveEntities(entities);
            this.UpdateWorld(Rc.Create(newWorld), Array.Empty<IDungeonCommand>());
        }

        /// <summary>
        /// rewind to a previous world state, if possible. Rewinding will add a new world state to the buffer, without
        /// rewinding the buffer itself. so rewinding `1` twice is NOT equivalent to rewinding `2` once. Rewinding `1` twice
        /// will result in the same world state as before the first rewind. 
        /// </summary>
        public void RewindBack(int backwardsSteps)
        {
            if(!CanUpdateWorld()) throw new InvalidOperationException("Cannot update world while already updating");
            if (backwardsSteps < 0)
            {
                throw new ArgumentException("Cannot rewind back a negative number of steps");
            }
            if (backwardsSteps == 0)
            {
                return;
            }

            if (backwardsSteps > OldWorldBufferCapacity)
            {
                throw new ArgumentException("Cannot rewind back more steps than the capacity of the buffer");
            }
            if (OldWorldBufferSize < backwardsSteps)
            {
                throw new ArgumentException("Cannot rewind back more steps than are stored in the buffer");
            }
            
            var worldToRewindTo = GetOldWorld(backwardsSteps).Clone();
            if (worldToRewindTo.Value == null)
            {
                throw new InvalidOperationException("Cannot rewind back to a null world");
            }
            var commands = Enumerable.Empty<IDungeonCommand>();
            this.UpdateWorld(worldToRewindTo, commands);
        }
    
        private void UpdateWorld(Rc<IDungeonWorld> newWorld, IEnumerable<IDungeonCommand> appliedCommands)
        {
            if(!CanUpdateWorld()) throw new InvalidOperationException("Cannot update world while already updating");
            AdvanceWorld(newWorld);
            Profiler.BeginSample("Update world");
            TriggerWorldUpdated(new DungeonUpdateEvent(_lastWorld.Value, newWorld.Value, appliedCommands.ToList()));
            Profiler.EndSample();
        }

        private void AdvanceWorld(Rc<IDungeonWorld> newWorld)
        {
            this.EnqueueOldWorld(this._lastWorld);
            this._lastWorld = _currentWorld;
            _currentWorld = newWorld;
        }
        
        private void EnqueueOldWorld(Rc<IDungeonWorld> world)
        {
            if (world.Value == null) return;
            if (OldWorldBufferCapacity == 0)
            {
                world.Dispose();
                return;
            }
            
            _lastWorldBuffer.Enqueue(world);
            if (_lastWorldBuffer.Count > _lastWorldBufferCapacity)
            {
                _lastWorldBuffer.Dequeue().Dispose();
            }
        }

        private Rc<IDungeonWorld> GetOldWorld(int backSteps)
        {
            if (backSteps == 1)
            {
                return _lastWorld;
            }

            backSteps--;
            
            // 1 is the last element (count - 1)
            // 2 is the 2nd to last element (count - 2)
            var indexIn = _lastWorldBuffer.Count - backSteps;
            return _lastWorldBuffer.ElementAt(indexIn);
        }
    
        public Vector3 GetCenter(Vector3Int coord)
        {
            if (useUnity2DAxises)
            {
                return new Vector3(coord.x, coord.z, -coord.y);
            }

            return new Vector3(coord.x, coord.y, coord.z);
        }
        public Vector3Int GetClosestToCenter(Vector3 pos)
        {
            if (useUnity2DAxises)
            {
                return new Vector3Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(-pos.z), Mathf.RoundToInt(pos.y));
            }
            return new Vector3Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y), Mathf.RoundToInt(pos.z));
        }


        private void OnDrawGizmosSelected()
        {
            // draw a wire cube for every tile in the world

            var origin = initialState.minBounds;
            var size = initialState.maxBounds - initialState.minBounds;
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    for (int z = 0; z < size.z; z++)
                    {
                        var coord = new Vector3Int(x, y, z) + origin;
                        var center = this.GetCenter(coord);
                        Gizmos.DrawWireCube(center, new Vector3(1, 1, 1));
                    }
                }
            }
        }

        public void AddUpdateListener(IRenderUpdate listener)
        {
            if(_updateListeners.TryGetValue(listener.RenderPriority, out var list))
            {
                list.Add(listener);
            }
            else
            {
                _updateListeners[listener.RenderPriority] = new List<IRenderUpdate>{listener};
            }
        }
        public void RemoveUpdateListener(IRenderUpdate listener)
        {
            if(_updateListeners.TryGetValue(listener.RenderPriority, out var list))
            {
                list.Remove(listener);
            }
        }
    
        private void TriggerWorldUpdated(DungeonUpdateEvent update)
        {
            Profiler.BeginSample("Trigger world updates");
            _updateCell.TryRun(async (cancel) =>
            {
                foreach (var kvp in _updateListeners)
                {
                    if(logUpdateRenderOrders) Debug.Log("Running update for priority " + kvp.Key);
                    var listenerBatch = kvp.Value;
                    var tasks = new UniTask[listenerBatch.Count];
                    // reverse order in case the update listeners remove themselves
                    for (int i = listenerBatch.Count - 1; i >= 0; i--)
                    {
                        tasks[i] = listenerBatch[i].RespondToUpdate(update, cancel);
                    }
                    await UniTask.WhenAll(tasks);
                }
                onAllRenderUpdatesComplete.Invoke();
            }, "Cannot update world while already updating");
            Profiler.EndSample();
        }
    }

    public interface IPostInitDungeonWorld
    {
        public IDungeonWorld PostInitialize(IDungeonWorld currentWorld);
    }
}