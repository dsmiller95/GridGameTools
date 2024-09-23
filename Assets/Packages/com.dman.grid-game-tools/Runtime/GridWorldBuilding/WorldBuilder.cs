using System;
using System.Collections.Generic;
using System.Linq;
using Dman.GridGameTools.Entities;
using Dman.GridGameTools.PathingData;
using Dman.GridGameTools.Random;
using Dman.Math;
using UnityEngine;

namespace Dman.GridGameTools.WorldBuilding
{
    public interface IDefaultFactoriesFactory
    {
        public Dictionary<string, Func<Vector3Int, IDungeonEntity>> GetDefaultFactories(uint seed);
    }

    public class WorldBuilder
    {
        public Dictionary<string, Func<Vector3Int, IDungeonEntity>> EntityFactories { get; }
        private readonly string _defaultChar;

        private WorldBuilder(IDefaultFactoriesFactory defaultEntities, string defaultChar = "-", uint seed = 0)
        {
            if(seed == 0) seed = (uint)UnityEngine.Random.Range(1, int.MaxValue);
            EntityFactories = defaultEntities.GetDefaultFactories(seed);
            _defaultChar = defaultChar;
        }

        public static WorldBuilder Create(IDefaultFactoriesFactory defaultEntities, string defaultChar = "-", uint seed = 0, IEnumerable<(string, Func<Vector3Int, IDungeonEntity>)> otherFactories = null)
        {
            var baseBuilder =  new WorldBuilder(defaultEntities, defaultChar, seed);

            if (otherFactories == null) return baseBuilder;
            
            foreach (var factory in otherFactories)
            {
                baseBuilder.EntityFactories[factory.Item1] = factory.Item2;
            }

            return baseBuilder;
        }

        /// <summary>
        /// build to a world with certain values set to defaults
        /// </summary>
        /// <returns></returns>
        public IDungeonWorld BuildToWorld(WorldBuildString characterMap, uint seed = 0,
            IEnumerable<IWorldComponent> components = null,
            IEnumerable<ICreateDungeonComponent> componentCreators = null,
            bool shuffleEntities = false)
        {
            var allEntities = Build(Vector3Int.zero, characterMap);
            if (shuffleEntities)
            {
                var entityList = allEntities.ToList();
                var rngSeed = seed == 0 ? (uint)UnityEngine.Random.Range(1, int.MaxValue) : seed;
                var rng = new GridRandomGen(rngSeed);
                rng.Shuffle(entityList);
                allEntities = entityList;
            }
            var bounds = new DungeonBounds(Vector3Int.zero, characterMap.Size());
            var pathingData = new DungeonPathingData(bounds, playerPosition: Vector3Int.zero);
            var allComponents = components?.ToList() ?? new List<IWorldComponent>(1);
            allComponents.Add(pathingData);
            if (componentCreators != null)
            {
                var creationContext = new WorldComponentCreationContext
                {
                    WorldBounds = bounds
                };
                foreach (var creator in componentCreators)
                {
                    allComponents.AddRange(creator.CreateComponents(creationContext));
                }
            }
            
            return DungeonWorld.CreateEmpty(seed, allComponents)
                .AddEntities(allEntities).world;
        }

        private IEnumerable<IDungeonEntity> Build(Vector3Int relativeOffset, WorldBuildString characterMap)
        {
            var entities = new List<IDungeonEntity>();
            var size = characterMap.Size();
            var characters = characterMap.GetInXYZ();
            foreach (Vector3Int pos in VectorUtilities.IterateAllIn(size))
            {
                var character = characters[pos] ?? _defaultChar;
                if (!EntityFactories.TryGetValue(character, out var factory)) continue;
                var position = pos + relativeOffset;
                var entity = factory(position);
                entities.Add(entity);
            }
            
            return entities;
        }
    }
}