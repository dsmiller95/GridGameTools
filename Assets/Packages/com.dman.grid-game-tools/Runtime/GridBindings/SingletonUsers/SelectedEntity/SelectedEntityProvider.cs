using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Dman.Utilities;
using Dman.Utilities.Logger;
using JetBrains.Annotations;
using UnityEngine;

namespace Dman.GridGameBindings.SelectedEntity
{
    public interface ISelectedEntityProvider
    {
        /// <summary>
        /// Allows setting the selected entity externally, for example, from a mouseclick
        /// </summary>
        public EntityId SelectedEntity { get; set; }
        /// <summary>
        /// When set to true, will allow the <see cref="SelectedEntity"/> to be changed. When false,
        /// will cache the last-set value for <see cref="SelectedEntity"/> and apply it after set back to true.
        /// The last-set value will not be applied instantaneously, it will wait until the next Update
        /// </summary>
        /// <remarks>
        /// Useful when you want to prevent the selected entity from being changed during a specific operation.
        /// For example, when the user is in the middle of an operation, or when the world is rendering updates from a previous change.
        /// </remarks>
        public bool AllowSelectedEntitySwitch { get; set; }
        /// <summary>
        /// Emits after the selected entity changes, with the newly selected entity id
        /// </summary>
        public event Action<EntityId> SelectedEntityChanged;
    }

    /// <summary>
    /// Implement this class and add it to a child of the <see cref="SelectedEntityProvider"/> in order to
    /// filter which entities are allowed to be selected when choosing a default pre-selected entity
    /// </summary>
    public interface IFilterSelectedEntity
    {
        public bool IncludeEntity(IDungeonEntity entity);
    }

    [UnitySingleton]
    public class SelectedEntityProvider : MonoBehaviour, ISelectedEntityProvider, IRenderUpdateAndSwap
    {
        private IBindExternalUpdates Updater => SingletonLocator<IBindExternalUpdates>.Instance;
    
        private EntityId selectedEntity;
    
        public EntityId SelectedEntity
        {
            get => selectedEntity;
            set
            {
                if (!AllowSelectedEntitySwitch)
                {
                    Log.Warning("Tried to set selected entity when setting selected entity is disabled");
                    pendingNextSelectedEntity = value;
                    return;
                }
                if (selectedEntity == value) return;
                selectedEntity = value;
                SelectedEntityChanged?.Invoke(selectedEntity);
            }
        }

        public bool AllowSelectedEntitySwitch
        {
            get => _allowSelectedEntitySwitch;
            set => _allowSelectedEntitySwitch = value;
        }

        [SerializeField] private bool _allowSelectedEntitySwitch;

        [CanBeNull] private EntityId pendingNextSelectedEntity;
        public event Action<EntityId> SelectedEntityChanged;
    
        private IFilterSelectedEntity[] filters = Array.Empty<IFilterSelectedEntity>();

        private void Awake()
        {
            pendingNextSelectedEntity = null;
        }

        private void OnEnable()
        {
            filters = GetComponentsInChildren<IFilterSelectedEntity>();
            Updater.AddUpdateListenerWithSwap(this);
        }
        private void OnDisable()
        {
            Updater?.RemoveUpdateListenerWithSwap(this);
        }

        public int RenderPriority => 0;

        private void Update()
        {
            if (pendingNextSelectedEntity != null && AllowSelectedEntitySwitch)
            {
                SelectedEntity = pendingNextSelectedEntity;
                pendingNextSelectedEntity = null;
            }
        }

        public void OnWorldWasSwapped()
        {
            var bestSelectedOption = FindBestSelectedId(DungeonWorldManagerSingleton.Instance.CurrentWorld, selectedEntity, filters);
            if (bestSelectedOption != selectedEntity)
            {
                Log.Info($"Selected entity changed from {selectedEntity} to {bestSelectedOption}");
                SelectedEntity = bestSelectedOption;
            }
        }
    
        public UniTask RespondToUpdate(DungeonUpdateEvent update, CancellationToken cancel)
        {
            var bestSelectedOption = FindBestSelectedId(update.NewWorld, selectedEntity, filters);
            if (bestSelectedOption != selectedEntity)
            {
                Log.Info($"Selected entity changed from {selectedEntity} to {bestSelectedOption}");
                SelectedEntity = bestSelectedOption;
            }
            return UniTask.CompletedTask;
        }

        private static EntityId FindBestSelectedId(IDungeonWorld world, EntityId existingId, IFilterSelectedEntity[] filters)
        {
            if (world == null)
            {
                return existingId;
            }

            if (existingId != null && world.EntityStore.GetEntity(existingId) != null)
            { // if the entity exists in the new world, don't try to find a new one.
                return existingId;
            }

            var targetEntities = world.EntityStore
                .AllEntitiesWithIds()
                .Where(entityPair =>
                {
                    return filters.All(x => x.IncludeEntity(entityPair.entity));
                })
                .ToList();
            switch (targetEntities.Count)
            {
                case 0:
                    var filterNames = string.Join(", ", filters.Select(x => x.GetType().Name));
                    Log.Error($"SelectedEntityProvider: No selectable entity found, with filters {filterNames}.");
                    return existingId;
            }

            return targetEntities.First().id;
        }
    }
}