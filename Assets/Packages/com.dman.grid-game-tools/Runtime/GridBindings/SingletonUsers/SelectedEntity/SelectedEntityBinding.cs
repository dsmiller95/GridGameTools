using System.Threading;
using Cysharp.Threading.Tasks;
using Dman.Utilities;
using JetBrains.Annotations;
using UnityEngine;

public abstract class SelectedEntityBinding : MonoBehaviour, IRenderUpdate
{
    public abstract int RenderPriority { get; }
    protected ISelectedEntityProvider Select => SingletonLocator<ISelectedEntityProvider>.Instance;
    private IDungeonUpdater Updater => SingletonLocator<IBindExternalUpdates>.Instance;

    /// <summary>
    /// Flip to true at the beginning of SelectedEntityChanged in order to keep the world waiting.
    /// Will continue with the world render update after it flips back to false.
    /// </summary>
    protected bool KeepWorldUpdatePending { get; set; } 
    [CanBeNull] protected IDungeonWorld PreviousWorld { get; private set; }
    protected IDungeonWorld AgainstWorld { get; private set; }
    
    protected abstract void SelectedEntityChanged([CanBeNull] IDungeonEntity newEntity, bool didIdChange);
    
    private void OnEnable()
    {
        Updater.AddUpdateListener(this);
        Select.SelectedEntityChanged += OnEntityIdChanged;
    }
    private void OnDisable()
    {
        Updater?.RemoveUpdateListener(this);
        if (Select != null)
        {
            Select.SelectedEntityChanged -= OnEntityIdChanged;
        }
    }

    private void OnEntityIdChanged(EntityId id)
    {
        var world = AgainstWorld ?? DungeonWorldManagerSingleton.Instance.CurrentWorld;
        var newEntity = world?.EntityStore.GetEntity(id);
        this.SelectedEntityChanged(newEntity, true);
    }

    public async UniTask RespondToUpdate(DungeonUpdateEvent update, CancellationToken cancel)
    {
        await UniTask.NextFrame(cancel);

        var newEntity = update.NewWorld.EntityStore.GetEntity(Select.SelectedEntity);
        if (newEntity != null)
        {
            PreviousWorld = update.OldWorld;
            AgainstWorld = update.NewWorld;
        }
        else
        {
            PreviousWorld = null;
            newEntity = update.OldWorld?.EntityStore.GetEntity(Select.SelectedEntity);
            AgainstWorld = update.OldWorld;
        }
        this.SelectedEntityChanged(newEntity, false);
        if(KeepWorldUpdatePending) await UniTask.WaitUntil(() => KeepWorldUpdatePending == false, cancellationToken: cancel);
    }
}