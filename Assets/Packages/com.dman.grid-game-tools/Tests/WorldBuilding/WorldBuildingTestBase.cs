using System;
using System.Collections.Generic;
using System.Linq;
using Dman.Math;
using JetBrains.Annotations;
using NUnit.Framework;
using UnityEngine;

namespace GridDomain.Test
{
    public interface IEntityComparator
    {
        public object GetIdentifyingKey(IDungeonEntity entity);
    }

    public interface IWorldComponentFactory
    {
        public IEnumerable<IWorldComponent> GetComponents();
    }
    
    public abstract class WorldBuildingTestBase
    {
        protected abstract IDefaultFactoriesFactory DefaultEntitiesFactory { get; }
        protected abstract IEntityComparator EntityComparator { get; }
        protected abstract WorldBuildConfig DefaultBuildConfig { get; }
        
        protected IDungeonWorld World;
        protected WorldBuilder WorldBuilder;
        protected WorldBuildConfig LastUsedBuildConfig;

        [CanBeNull] private IEnumerable<IWorldComponent> components;
        protected void UseWorldComponents(IEnumerable<IWorldComponent> oneTimeComponents)
        {
            components = oneTimeComponents;
        }
        protected void UseWorldComponents(params IWorldComponent[] oneTimeComponents)
        {
            components = oneTimeComponents;
        }
        protected void UseWorldComponents(params IWorldComponentFactory[] componentFactories)
        {
            components = componentFactories.SelectMany(x => x.GetComponents()).ToList();
        }
        
        protected void CreateWorld(string characterMap, params (string, Func<Vector3Int, IDungeonEntity>)[] otherFactories)
        {
            CreateWorld(characterMap, seed: 0, otherFactories);
        }
        protected void CreateWorld(string characterMap, ulong seed = 0, params (string, Func<Vector3Int, IDungeonEntity>)[] otherFactories)
        {
            CreateWorld(
                (characterMap, DefaultBuildConfig),
                seed,
                otherFactories
            );
        }

        protected void CreateWorld(WorldBuildString characterMap, ulong seed = 0, params (string, Func<Vector3Int, IDungeonEntity>)[] otherFactories)
        {
            LastUsedBuildConfig = characterMap.BuildConfig;
            var defaultStr = '-'.ToString();
            WorldBuilder =  WorldBuilder.Create(DefaultEntitiesFactory, defaultStr, seed, otherFactories);
         
            //   N
            // W x E
            //   S
            var usedComponents = components;
            components = null;
            World = WorldBuilder.BuildToWorld(characterMap, seed, usedComponents);
        }
        protected EntityHandle<T> GetAtSingle<T>(Vector3Int position) where T: IDungeonEntity
        {
            return World.EntityStore.EntitiesAtOf<T>(position).Single();
        }
        protected EntityHandle<T> GetAtSingle<T>(int x, int y, int z) where T: IDungeonEntity
        {
            return GetAtSingle<T>(new Vector3Int(x, y, z));
        }
        
        protected EntityHandle<T> GetSingle<T>(Func<T, bool> filter = null) where T: IDungeonEntity
        {
            return World.EntityStore.AllEntitiesMatching(filter).Single();
        }
        protected EntityId GetSingleGeneric<T>(Func<T, bool> filter = null)
        {
            return World.EntityStore.AllEntityIdsMatching(filter).Single();
        }
        

        protected void AssertWorldMatches(string expectedMap, params object[] excludedObjects)
        {
            var identifierMaps = WorldBuilder.EntityFactories
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
                LastUsedBuildConfig,
                '-',
                identifierMaps
                );

            AssertEqIgnoringLineSep(expectedMap, worldString.OriginalCharacterMap);
        }
        private void AssertEqIgnoringLineSep(string expected, string actual)
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
            var bounds = World.Bounds;
            var actualCharsXYZ = new XyzGrid<char>(bounds.Size);

            var charsByIdentifier = identifiersToChars.ToDictionary(x => x.Item1, x => x.Item2);
            
            foreach (var arrayPoint in VectorUtilities.IterateAllIn(bounds.Size))
            {
                var worldPoint = arrayPoint + bounds.Min;
                
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

                char result = actualCharsAtPoint.Length switch
                {
                    0 => defaultChar,
                    > 1 => throw new Exception(
                        $"Multiple mapped entities on tile {worldPoint}. entities: {string.Join(", ", actualCharsAtPoint)}"),
                    _ => actualCharsAtPoint.Single()
                };

                actualCharsXYZ[arrayPoint] = result;
            }

            return WorldBuildString.BuildStringFromXYZ(actualCharsXYZ, buildOpts);
        }

        
        private object GetIdentifyingKey(IDungeonEntity entityToKey)
        {
            return EntityComparator.GetIdentifyingKey(entityToKey);
        }
    }
}