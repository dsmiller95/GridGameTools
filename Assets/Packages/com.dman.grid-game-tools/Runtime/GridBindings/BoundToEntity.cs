using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Profiling;

public abstract class BoundToEntity<T> : MonoBehaviour, IBoundToEntity, IRenderUpdate where T : IDungeonEntity
{
    public Type BoundType => typeof(T);

    public EntityHandle<T> Entity { get; private set; }
    public EntityId EntityId => Entity.Id;
    private string __idForInspector; // for visibility in the debug inspector
    private bool _isBound = false;
    private IDungeonUpdater _updater;
    private IDungeonToWorldContext _worldContext;

    protected abstract bool UnbindAfterInstantiate { get; }
    
    public IDungeonEntity GetEntityObjectGuessGeneric(IDungeonToWorldContext context) => GetEntityObjectGuess(context);
    
    [CanBeNull] protected abstract T GetEntityObjectGuess(IDungeonToWorldContext context);


    public bool TryBind(EntityId entity, IDungeonUpdater updater, IDungeonToWorldContext worldContext, IDungeonWorld world)
    {
        var handle = world.EntityStore.TryGetHandle<T>(entity);
        if (handle == null)
        {
            Debug.LogError($"Could not bind {this.name} to entity {entity}");
            return false;
        }
        Bind(handle, updater, worldContext);
        return true;
    }

    private void Bind(EntityHandle<T> entity, IDungeonUpdater updater, IDungeonToWorldContext worldContext)
    {
        if (_isBound)
        {
            Debug.LogError($"Can only bind once, {this.name} is already bound");
            return;
        }

        _worldContext = worldContext;
        _updater = updater;
        Entity = entity;
        this.__idForInspector = Entity.Id.ToString();
        OnBind();


        this._updater.AddUpdateListener(this);
    }

    private void OnDestroy()
    {
        if (!_isBound) return;
        this._updater.RemoveUpdateListener(this);
    }

    /// <summary>
    /// called exactly once, when the entity is created.
    /// </summary>
    protected virtual void OnBind()
    {
    }

    public async UniTask RespondToUpdate(DungeonUpdateEvent update, CancellationToken cancel)
    {
        _currentProcessingEvent = update;
        Profiler.BeginSample($"{typeof(T).Name} Binding: Get TakenActions");
        // respond to all commands in parallel, before updating the entity
        var takenActions = update.AppliedCommands
            .Where(x => x.ActionTaker == EntityId)
            .Select(x => OnTookAction(x, cancel));
        Profiler.EndSample();
        
        await UniTask.WhenAll(takenActions);
        
        Profiler.BeginSample($"{typeof(T).Name} Binding: Invoke on updated");
        var oldEntity = update.OldWorld?.EntityStore.GetEntity(this.Entity);
        var newEntity = update.NewWorld.EntityStore.GetEntity(this.Entity);
        // what if we want more than just reference comparison?
        var task = (newEntity == oldEntity)
            ? OnWorldChangedWithNoEntityChange(newEntity)
            : OnBindingChanged(oldEntity, newEntity, _worldContext, cancel);
        Profiler.EndSample();
        await task;
        _currentProcessingEvent = null;
    }

    [CanBeNull] private DungeonUpdateEvent _currentProcessingEvent;
    [CanBeNull] protected DungeonUpdateEvent CurrentProcessingEvent => _currentProcessingEvent;

    protected virtual UniTask OnWorldChangedWithNoEntityChange(T currentEntity) => UniTask.CompletedTask;

    protected virtual UniTask OnTookAction(IDungeonCommand command, CancellationToken cancel)
    {
        return UniTask.CompletedTask;
    }

    /// <summary>
    /// called whenever a change occurs to the entity. Only after the entity has been created..
    /// </summary>
    /// <param name="oldEntity">When null, is an entity creation event</param>
    /// <param name="newEntity">When null, is an entity destruction event</param>
    /// <param name="context"></param>
    /// <param name="cancel"></param>
    protected virtual UniTask OnBindingChanged([CanBeNull] T oldEntity, [CanBeNull] T newEntity, IDungeonToWorldContext context, CancellationToken cancel)
    {
        if (newEntity == null)
        {
            Destroy(this.gameObject);
            return UniTask.CompletedTask;
        }
        if(oldEntity == null)
        { // hard cut to set position on creation
            SetPosition(newEntity, context);
            if (UnbindAfterInstantiate)
            {
                this._updater.RemoveUpdateListener(this);
            }
        }
        // Debug.Log($"Binding for {this.name} was updated");
        return UniTask.CompletedTask;
    }

    protected void SetPosition(T newEntity, IDungeonToWorldContext context)
    {
        var (pos, rot) = context.GetPosRot(newEntity.Coordinate);
        this.transform.SetPositionAndRotation(pos, rot);
    }
}
