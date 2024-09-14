using System.Threading;
using Cysharp.Threading.Tasks;
using Dman.GridGameTools;
using Dman.GridGameTools.Entities;
using Dman.Utilities;
using Dman.Utilities.Logger;
using JetBrains.Annotations;
using UnityEngine;

namespace Dman.GridGameBindings.SelectedEntity
{
    public abstract class SelectedEntityBinding : MonoBehaviour, IRenderUpdate
    {
        [SerializeField] private bool logIfSelectedNull = false;
        
        public abstract int RenderPriority { get; }
        protected ISelectedEntityProvider Select => SingletonLocator<ISelectedEntityProvider>.Instance;
        private IDungeonUpdater Updater => SingletonLocator<IBindExternalUpdates>.Instance;

        /// <summary>
        /// Flip to true at the beginning of SelectedEntityChanged in order to keep the world waiting.
        /// Will continue with the world render update after it flips back to false.
        /// </summary>
        protected bool KeepWorldUpdatePending { get; set; } 
        [CanBeNull] protected IDungeonWorld PreviousWorld { get; private set; }
        protected IDungeonWorld CurrentWorld { get; private set; }
        protected IDungeonWorld AgainstWorld => CurrentWorld ?? PreviousWorld;
    
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
            var world = CurrentWorld ?? DungeonWorldManagerSingleton.Instance.CurrentWorld;
            var newEntity = world?.EntityStore.GetEntity(id);
            if (newEntity == null)
            {
                newEntity = PreviousWorld?.EntityStore.GetEntity(id);
            }
            this.SelectedEntityChanged(newEntity, true);
        }

        public async UniTask RespondToUpdate(DungeonUpdateEvent update, CancellationToken cancel)
        {
            await UniTask.NextFrame(cancel);

            if (logIfSelectedNull && Select.SelectedEntity == null)
            {
                Log.Warning("Selected entity is null", this);
            }

            var id = Select.SelectedEntity;
            var newEntity = id == null ? null : update.NewWorld.EntityStore.GetEntity(id);
            if (newEntity != null)
            {
                PreviousWorld = update.OldWorld;
                CurrentWorld = update.NewWorld;
            }
            else
            {
                PreviousWorld = update.OldWorld;
                CurrentWorld = null;
            }

            if (newEntity == null)
            {
                newEntity = id == null ? null : update.OldWorld?.EntityStore.GetEntity(id);
            }
            this.SelectedEntityChanged(newEntity, false);
            if(KeepWorldUpdatePending) await UniTask.WaitUntil(() => KeepWorldUpdatePending == false, cancellationToken: cancel);
        }
    }
}