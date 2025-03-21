using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Dman.GridGameTools;
using Dman.GridGameTools.Entities;
using Dman.GridGameTools.PathingData;
using Dman.GridGameTools.Random;
using Dman.GridGameTools.WorldBuilding;
using JetBrains.Annotations;
using NUnit.Framework;
using UnityEngine;
using AssertionException = UnityEngine.Assertions.AssertionException;

namespace GridDomain.Test
{
    public interface IEntityComparator
    {
        public object GetIdentifyingKey(IDungeonEntity entity);
    }
    
    public abstract class WorldBuildingTestBase
    {
        protected abstract IDefaultFactoriesFactory DefaultEntitiesFactory { get; }
        protected abstract IEntityComparator EntityComparator { get; }
        protected abstract WorldBuildConfig DefaultBuildConfig { get; }
        
        protected IDungeonWorld World;

        private WorldBuilderPatternResult _worldBuildResult;
        protected WorldBuilderPatternResult WorldBuildResult
        {
            get => _worldBuildResult;
            set
            {
                _worldBuildResult = value;
                World = _worldBuildResult.World;
            }
        }

        [Obsolete("Use LastWorldBuildResult instead")]
        protected WorldBuildConfig LastUsedBuildConfig => WorldBuildResult.WorldBuildConfig;
        protected bool ShouldShuffleEntities = false;

        [Obsolete("Use GetBuilder().AddComponents(...).BuildInto(this) instead")]
        [CanBeNull] private IEnumerable<IWorldComponent> components;
        [Obsolete("Use GetBuilder().AddComponents(...).BuildInto(this) instead")]
        [CanBeNull] private IEnumerable<ICreateDungeonComponent> componentFactories;

        [Obsolete("Use GetBuilder().AddComponents(...).BuildInto(this) instead")]
        protected void UseWorldComponents(IEnumerable<IWorldComponent> oneTimeComponents)
        {
            components = oneTimeComponents;
            componentFactories = null;
        }
        [Obsolete("Use GetBuilder().AddComponents(...).BuildInto(this) instead")]
        protected void UseWorldComponents(params IWorldComponent[] oneTimeComponents)
        {
            components = oneTimeComponents;
            componentFactories = null;
        }
        [Obsolete("Use GetBuilder().AddComponents(...).BuildInto(this) instead")]
        protected void UseWorldComponents(params ICreateDungeonComponent[] oneTimeComponentFactories)
        {
            componentFactories = oneTimeComponentFactories;
            components = null;
        }

        [Obsolete("Use GetBuilder().AddComponents(...).BuildInto(this) instead")]
        protected void AddWorldComponents(params ICreateDungeonComponent[] oneTimeComponentFactories)
        {
            componentFactories = componentFactories?.Concat(oneTimeComponentFactories).ToList() ?? oneTimeComponentFactories.ToList();
        }

        protected void CreateWorld(string characterMap)
        {
            WorldBuildResult = GetBuilder()
                .WithMap(characterMap)
                .Build();
        }
        
        [Obsolete("Use GetBuilder().WithXYZ(...).BuildInto(this) instead")]
        protected void CreateWorld(string characterMap, params (string, Func<Vector3Int, IDungeonEntity>)[] otherFactories)
        {
            CreateWorld(characterMap, seed: 0, otherFactories);
        }

        [Obsolete("Use GetBuilder().WithXYZ(...).BuildInto(this) instead")]
        protected void CreateWorld(string characterMap, uint seed = 0, params (string, Func<Vector3Int, IDungeonEntity>)[] otherFactories)
        {
            CreateWorld(
                (characterMap, DefaultBuildConfig),
                seed,
                otherFactories
            );
        }

        [Obsolete("Use GetBuilder().WithXYZ(...).BuildInto(this) instead")]
        protected void CreateWorld(WorldBuildString characterMap, uint seed = 0, params (string, Func<Vector3Int, IDungeonEntity>)[] otherFactories)
        {
            var usedComponents = components;
            components = null;
            var usedComponentFactories = componentFactories;
            componentFactories = null;

            WorldBuildResult = GetBuilder()
                .WithSeed(seed)
                .AddFactories(otherFactories)
                .WithMap(characterMap)
                .AddComponents(usedComponents)
                .AddComponents(usedComponentFactories)
                .Build();
        }
        
        protected WorldBuilder GetBuilder()
        {
            var builder =  WorldBuilder.Create()
                .WithBuildConfig(DefaultBuildConfig)
                .WithDefaultEntities(DefaultEntitiesFactory)
                .WithDefaultChar("-")
                .WithSeed(0)
                .WithShuffleEntities(ShouldShuffleEntities);
            builder = ApplyTestWorldBuilderDefaults(builder);
            return builder;
        }

        protected virtual WorldBuilder ApplyTestWorldBuilderDefaults(WorldBuilder builder) => builder;
        
        protected EntityHandle<T> GetAtSingle<T>(Vector3Int position) where T: IDungeonEntity
        {
            return World.EntityStore.EntitiesAtOf<T>(position).Single();
        }
        protected EntityHandle<T> GetAtSingle<T>(int x, int y, int z) where T: IDungeonEntity
        {
            return GetAtSingle<T>(new Vector3Int(x, y, z));
        }

        protected EntityId GetAtSingle(Vector3Int position)
        {
            return World.EntityStore.GetEntitiesAt(position).Single();
        }
        
        protected EntityHandle<T> GetSingle<T>(Func<T, bool> filter = null) where T: IDungeonEntity
        {
            return World.EntityStore.AllEntitiesMatching(filter).Single();
        }
        
        protected EntityId GetSingleGeneric<T>(Func<T, bool> filter = null) => AssertSingleGeneric(filter);

        protected EntityId AssertSingleGeneric<T>(Func<T, bool> filter = null, string ifFailMessage = null)
        {
            var matched = World.EntityStore.AllEntityIdsMatching(filter).ToList();
            switch (matched.Count)
            {
                case 0:
                    throw new AssertionException("Expected exactly 1 entity, found 0", ifFailMessage);
                case > 1:
                    throw new AssertionException($"Expected exactly 1 entity, found {matched.Count}", ifFailMessage);
            }

            return matched.Single();
        }
        
        protected void AssertNone<T>(Func<T, bool> filter = null, string ifFailMessage = null)
        {
            var matchedCount = World.EntityStore.AllEntityIdsMatching(filter).Count();
            switch (matchedCount)
            {
                case >= 1:
                    throw new AssertionException($"Expected exactly 0 entities, found {matchedCount}", ifFailMessage);
            }
        }
        
        protected void AssertWorldMatches(string expectedMap, params object[] excludedObjects)
        {
            var identifierMaps = WorldBuildResult.FinalEntityFactories
                .Select(x =>
                {
                    if (x.Key.Length != 1) throw new Exception("must have single character keys");
                    IDungeonEntity generatedEntity = x.Value(Vector3Int.zero);
                    object identifyingKey = GetIdentifyingKey(generatedEntity);
                    if (excludedObjects.Contains(identifyingKey)) return ((object, char)?)null;
                    return (identifyingKey, x.Key[0]);
                })
                .Where(x => x.HasValue)
                .Select(x => x.Value)
                .ToArray();
            WorldBuildString worldString = ToWorldString(
                WorldBuildResult.WorldBuildConfig,
                '-',
                identifierMaps
                );

            AssertEqIgnoringLineSep(expectedMap, worldString.OriginalCharacterMap);
        }
        protected static void AssertEqIgnoringLineSep(string expected, string actual)
        {
            var trimmedExpected = expected.Trim().Replace("\r\n", "\n");
            var trimmedActual = actual.Trim().Replace("\r\n", "\n");
            if (!trimmedExpected.Equals(trimmedActual))
            {
                Assert.Fail($"Expected \n{trimmedExpected}\n Actual \n{trimmedActual}");
            }
        }

        private WorldBuildString ToWorldString(
            WorldBuildConfig buildOpts,
            char defaultChar = '-',
            params (object, char)[] identifiersToChars)
        {
            var charsByIdentifier = identifiersToChars.ToDictionary(x => x.Item1, x => x.Item2);
            var pathingData = World.Components.AssertGet<IDungeonPathingDataBaked>();
            return ToWorldStringByQuery(buildOpts, pathingData.Bounds, (worldPoint) =>
            {
                var entities = World.EntityStore.GetEntityObjectsAt(worldPoint);
                var actualCharsAtPoint = entities
                    .Select(x =>
                    {
                        var identifier = GetIdentifyingKey(x);
                        if (identifier != null && charsByIdentifier.TryGetValue(identifier, out var charAt))
                        {
                            return charAt;
                        }

                        return (char?)null;
                    }).Where(x => x.HasValue).Select(x => x.Value).ToArray();

                return actualCharsAtPoint.Length switch
                {
                    0 => defaultChar,
                    > 1 => throw new Exception(
                        $"Multiple mapped entities on tile {worldPoint}. entities: {string.Join(", ", actualCharsAtPoint)}"),
                    _ => actualCharsAtPoint.Single()
                };
            });
        }
        
        protected static WorldBuildString ToWorldStringByQuery(
            WorldBuildConfig buildOpts,
            DungeonBounds bounds,
            Func<Vector3Int, char> coordToCharMap)
        {
            return WorldBuild.ToWorldStringByQuery(buildOpts, bounds, coordToCharMap);
        }

        
        private object GetIdentifyingKey(IDungeonEntity entityToKey)
        {
            return EntityComparator.GetIdentifyingKey(entityToKey);
        }

        /// <summary>
        /// Gets a seed that is unique to the calling method.
        /// </summary>
        /// <param name="caller"></param>
        /// <returns></returns>
        protected uint GetSeed([CallerMemberName] string caller = null)
        {
            return caller.ToSeed();
        }
    }
}