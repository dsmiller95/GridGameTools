using System;
using System.Collections.Generic;
using System.Linq;
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
        public IDungeonWorld BuildToWorld(WorldBuildString characterMap, uint seed = 0, IEnumerable<IWorldComponent> components = null)
        {
            var allEntities = Build(Vector3Int.zero, characterMap);
            var bounds = new DungeonBounds(Vector3Int.zero, characterMap.Size());
            var pathingData = new DungeonPathingData(bounds, playerPosition: Vector3Int.zero);
            components = components?.Append(pathingData) ?? new[] {pathingData};
            
            return DungeonWorld.CreateEmpty(seed, components)
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