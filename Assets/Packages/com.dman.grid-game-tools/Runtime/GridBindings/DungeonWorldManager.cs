using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using GridRandom;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Profiling;
using Object = UnityEngine.Object;


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
    public IDungeonWorld CurrentWorld { get; private set; }

    private IDungeonWorld _lastWorld;
    
    [SerializeField] private InitialDungeonState initialState = new InitialDungeonState();
    [SerializeField] private Transform worldParent;
    [SerializeField] public UnityEvent onAllRenderUpdatesComplete;
    
    [Header("Debug")]
    [SerializeField] public bool logTopLevelCommands = false;
    
    private SortedDictionary<int, List<IRenderUpdate>> _updateListeners = new ();
    private AsyncFnOnceCell _updateCell;

    public GridRandomGen SpawningRng { get; private set; }
    public Transform EntityParent => worldParent;
    
    private void Awake()
    {
        SpawningRng = new GridRandomGen(initialState.SeedOrRngIfDefault);
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
        CurrentWorld = initialWorld;
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
        this.UpdateWorld(newWorld, modifiedCommands);
    }
    
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
        
        this.UpdateWorld(newWorld, modifiedCommands);
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
        this.UpdateWorld(newWorld, Array.Empty<IDungeonCommand>());
    }
    
    public void DespawnEntities(params EntityId[] entities)
    {
        if(!CanUpdateWorld()) throw new InvalidOperationException("Cannot update world while already updating");
        
        var newWorld = CurrentWorld.RemoveEntities(entities);
        this.UpdateWorld(newWorld, Array.Empty<IDungeonCommand>());
    }
    
    private void UpdateWorld(IDungeonWorld newWorld, IEnumerable<IDungeonCommand> appliedCommands)
    {
        if(!CanUpdateWorld()) throw new InvalidOperationException("Cannot update world while already updating");
        AdvanceWorld(newWorld);
        Profiler.BeginSample("Update world");
        TriggerWorldUpdated(new DungeonUpdateEvent(_lastWorld, newWorld, appliedCommands.ToList()));
        Profiler.EndSample();
    }

    private void AdvanceWorld(IDungeonWorld newWorld)
    {
        this._lastWorld?.Dispose();
        this._lastWorld = CurrentWorld;
        CurrentWorld = newWorld;
    }
    
    public Vector3 GetCenter(Vector3Int coord)
    {
        return new Vector3(coord.x, coord.y, coord.z);
    }
    public Vector3Int GetClosestToCenter(Vector3 pos)
    {
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
                Debug.Log("Running update for priority " + kvp.Key);
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