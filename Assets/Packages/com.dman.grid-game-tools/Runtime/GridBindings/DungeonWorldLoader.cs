using System;
using System.Collections.Generic;
using System.Linq;
using Dman.GridGameTools;
using Dman.GridGameTools.Commands;
using Dman.GridGameTools.Entities;
using Dman.GridGameTools.PathingData;
using Dman.GridGameTools.Random;
using UnityEngine;

namespace Dman.GridGameBindings
{
    public interface IApplyCommandPostWorldLoad
    {
        public IDungeonCommand PostWorldLoadCommand { get; }
    }

    /// <summary>
    /// Collects all Bindings which are child to this game object.
    /// Will initialize the dungeon world singleton with a world containing these entities.
    /// </summary>
    public class DungeonWorldLoader : MonoBehaviour
    {
        [SerializeField]
        private Vector3Int maxBoundsExtension = new Vector3Int(0, 2, 0);
        
        [Tooltip("Optional. when set, will search for game objects which are children of this gameobject.")]
        [SerializeField] private GameObject entitiesParent; 
    
        public Transform WorldComponentCreators => transform;
        
        private DungeonWorldManager WorldManager => _worldManager ? _worldManager : (_worldManager = GetComponent<DungeonWorldManager>());
        private DungeonWorldManager _worldManager;
    
        public struct EntityGuessAndBinding
        {
            public IDungeonEntity entity;
            public IBoundToEntity binding;
        }

        private void Start()
        {
            var seed = WorldManager.SpawningRng.Fork(nameof(DungeonWorldLoader).ToSeed());
            var newWorld = CreateAndAddEntitiesFromAllBindings(WorldManager, WorldManager, seed.NextState());
            WorldManager.TakeInitialWorldState(newWorld);
        }

        [Obsolete]
        [SerializeField] private bool createPathingExtraAlways = true;
        public IDungeonWorld CreateAndAddEntitiesFromAllBindings(
            IDungeonToWorldContext context,
            IDungeonUpdater updater,
            uint seed = 0)
        {
            var actualParent = entitiesParent ? entitiesParent : gameObject;
            var allBindings = actualParent.GetComponentsInChildren<IBoundToEntity>();
            var allPairs = ExtractEntityAndBindingPairs(context, allBindings);

            var bounds = new DungeonBounds(Vector3Int.zero, Vector3Int.one);
            foreach (EntityGuessAndBinding pair in allPairs)
            {
                bounds = bounds.Extend(pair.entity.Coordinate.Position);
            }

            bounds = bounds.Extend(bounds.Max + maxBoundsExtension);

            var creationContext = new WorldComponentCreationContext
            {
                WorldBounds = bounds
            };
            var components = GetComponents(creationContext);
            var world = DungeonWorld.CreateEmpty(seed, components);

            (IDungeonWorld newWorld, var entities) = world.AddEntities(allPairs.Select(x => x.entity));
            var postWorldCommand = GetComponentInChildren<IApplyCommandPostWorldLoad>()?.PostWorldLoadCommand;
            if (postWorldCommand != null)
            {
                newWorld = newWorld.ApplyCommand(postWorldCommand, andDispose: true);
            }
            var entityIds = entities.ToList();
            for (int i = 0; i < entityIds.Count; i++)
            { 
                var bound = allPairs[i].binding.TryBind(entityIds[i], updater, context, newWorld);
                if (!bound)
                {
                    Debug.LogError($"Failed to bind entity {entityIds[i]} to {allPairs[i].binding}");
                }
            }

            return newWorld;
        }
    
        private List<EntityGuessAndBinding> ExtractEntityAndBindingPairs(IDungeonToWorldContext context, IEnumerable<IBoundToEntity> allBindings)
        {
            var entitiesAndBindings = new List<EntityGuessAndBinding>();
            foreach (IBoundToEntity binding in allBindings)
            {
                IDungeonEntity generatedEntity = binding.GetEntityObjectGuessGeneric(context, WorldComponentCreators);
                if (generatedEntity == null)
                {
                    Debug.LogError($"Could not guess entity for {binding} of type {binding.GetType()} and name {(binding as UnityEngine.Object)?.name}");
                    continue;
                }
                entitiesAndBindings.Add(new EntityGuessAndBinding
                {
                    entity = generatedEntity,
                    binding = binding,
                });
            }

            return entitiesAndBindings;
        }

    
        private IEnumerable<IWorldComponent> GetComponents(WorldComponentCreationContext context)
        {
            var componentCreators = WorldComponentCreators.GetComponents<ICreateDungeonComponent>();
            if (createPathingExtraAlways)
            {
                yield return new DungeonPathingData(context.WorldBounds, playerPosition: Vector3Int.zero);
            }
            foreach (var creator in componentCreators)
            {
                foreach (var component in creator.CreateComponents(context))
                {
                    yield return component;
                }
            }
        }
    }
}