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
    public record WorldBuilder
    {
        private WorldBuilder() { }

        public static WorldBuilder Create() => new();

        private IDefaultFactoriesFactory DefaultEntities { get; set; }
        private string DefaultChar { get; set; } = "-";
        private uint Seed { get; set; } = 0;
        private WorldBuildConfig BuildConfig { get; set; } = WorldBuildConfig.Default;
        private IEnumerable<(string, Func<Vector3Int, IDungeonEntity>)> OtherFactories { get; set; } = Enumerable.Empty<(string, Func<Vector3Int, IDungeonEntity>)>();
        private string CharacterMap { get; set; } = null;
        private IEnumerable<IWorldComponent> Components { get; set; } = Enumerable.Empty<IWorldComponent>();
        private IEnumerable<ICreateDungeonComponent> ComponentCreators { get; set; } = Enumerable.Empty<ICreateDungeonComponent>();
        private bool ShuffleEntities { get; set; } = false;
        
        public WorldBuilder WithDefaultEntities(IDefaultFactoriesFactory defaultEntities) =>
            this with { DefaultEntities = defaultEntities };
        
        public WorldBuilder WithDefaultChar(string defaultChar) =>
            this with { DefaultChar = defaultChar };
        
        public WorldBuilder WithSeed(uint seed) =>
            this with { Seed = seed };
        
        public WorldBuilder WithOtherFactories(params (string, Func<Vector3Int, IDungeonEntity>)[] otherFactories) =>
            otherFactories == null ? this : this with { OtherFactories = otherFactories };
        public WorldBuilder WithOtherFactories(IEnumerable<(string, Func<Vector3Int, IDungeonEntity>)> otherFactories) =>
            otherFactories == null ? this : this with { OtherFactories = otherFactories };
        
        public WorldBuilder WithBuildConfig(WorldBuildConfig defaultBuildConfig) =>
            this with { BuildConfig = defaultBuildConfig };
        public WorldBuilder WithMap(string characterMap) =>
            this with { CharacterMap = characterMap };
        
        public WorldBuilder WithMap(string characterMap, WorldBuildConfig buildConfig) =>
            this with { CharacterMap = characterMap, BuildConfig = buildConfig };
        public WorldBuilder WithMap(WorldBuildString characterMap) => 
            characterMap == null ? this : this with { CharacterMap = characterMap.OriginalCharacterMap, BuildConfig = characterMap.BuildConfig };
        
        
        public WorldBuilder WithComponents(IEnumerable<IWorldComponent> components) =>
            components == null ? this : this with { Components = components };
        public WorldBuilder AddComponents(IEnumerable<IWorldComponent> components) =>
            components == null ? this : this with { Components = Components.Concat(components) };
        public WorldBuilder AddComponents(params IWorldComponent[] components) =>
            components == null ? this : this with { Components = Components.Concat(components) };
        public WorldBuilder WithComponents(IEnumerable<ICreateDungeonComponent> componentCreators) =>
            componentCreators == null ? this : this with { ComponentCreators = componentCreators };
        public WorldBuilder AddComponents(IEnumerable<ICreateDungeonComponent> componentCreators) =>
            componentCreators == null ? this : this with { ComponentCreators = ComponentCreators.Concat(componentCreators) };
        public WorldBuilder AddComponents(params ICreateDungeonComponent[] componentCreators) =>
            componentCreators == null ? this : this with { ComponentCreators = ComponentCreators.Concat(componentCreators) };
        
        public WorldBuilder WithShuffleEntities(bool shuffleEntities) =>
            this with { ShuffleEntities = shuffleEntities };

        [Obsolete("Use Create() and then configure the builder with the fluent API instead.")]
        public static WorldBuilder Create(IDefaultFactoriesFactory defaultEntities, string defaultChar = "-", uint seed = 0, IEnumerable<(string, Func<Vector3Int, IDungeonEntity>)> otherFactories = null)
        {
            return Create()
                .WithDefaultEntities(defaultEntities)
                .WithDefaultChar(defaultChar)
                .WithSeed(seed)
                .WithOtherFactories(otherFactories);
        }
        
        [Obsolete("Use Create() and then configure the builder with the fluent API instead.")]
        public IDungeonWorld BuildToWorld(
            WorldBuildString characterMap, uint seed = 0,
            IEnumerable<IWorldComponent> components = null,
            IEnumerable<ICreateDungeonComponent> componentCreators = null,
            bool shuffleEntities = false)
        {
            var newBuilder = this
                .WithMap(characterMap)
                .WithSeed(seed)
                .WithComponents(components)
                .WithComponents(componentCreators)
                .WithShuffleEntities(shuffleEntities);
            return newBuilder.Build().World;
        }
        
        public WorldBuilderPatternResult Build()
        {
            if (DefaultEntities == null) throw new ArgumentNullException(nameof(DefaultEntities), "DefaultEntities cannot be null.");
            if (CharacterMap == null) throw new ArgumentNullException(nameof(CharacterMap), "CharacterMap cannot be null.");
            
            // all collections must be non-null
            if (OtherFactories == null) throw new ArgumentNullException(nameof(OtherFactories), "OtherFactories cannot be null.");
            if (Components == null) throw new ArgumentNullException(nameof(BuildConfig), "Components cannot be null.");
            if (ComponentCreators == null) throw new ArgumentNullException(nameof(ComponentCreators), "ComponentCreators cannot be null.");

            var characterMap = new WorldBuildString(CharacterMap, BuildConfig);
            
            var realizedSeed = Seed == 0 ? (uint)UnityEngine.Random.Range(1, int.MaxValue) : Seed;
            var entityFactories = DefaultEntities.GetDefaultFactories(realizedSeed);
            foreach (var factory in OtherFactories)
            {
                entityFactories[factory.Item1] = factory.Item2;
            }
            
            var allEntities = CreateEntities(characterMap, DefaultChar, entityFactories);
            
            if (ShuffleEntities)
            {
                allEntities = ShuffleEntityList(allEntities, realizedSeed);
            }
            
            var bounds = new DungeonBounds(Vector3Int.zero, characterMap.Size());
            var pathingData = new DungeonPathingData(bounds, playerPosition: Vector3Int.zero);
            var allComponents = Components?.ToList() ?? new List<IWorldComponent>(1);
            allComponents.Add(pathingData);
            
            var creationContext = new WorldComponentCreationContext { WorldBounds = bounds };
            foreach (var creator in ComponentCreators)
            {
                allComponents.AddRange(creator.CreateComponents(creationContext));
            }
            
            
            IDungeonWorld world = DungeonWorld.CreateEmpty(Seed, allComponents)
                .AddEntities(allEntities).world;

            return new WorldBuilderPatternResult(world, entityFactories, BuildConfig);
        }
        
        private static IEnumerable<IDungeonEntity> ShuffleEntityList(IEnumerable<IDungeonEntity> entities, uint seed)
        {
            var entityList = entities.ToList();
            var rng = new GridRandomGen(seed);
            rng.Shuffle(entityList);
            return entityList;
        }
        
        private static IEnumerable<IDungeonEntity> CreateEntities(
            WorldBuildString characterMap,
            string defaultChar,
            IReadOnlyDictionary<string, Func<Vector3Int, IDungeonEntity>> entityFactories)
        {
            var entities = new List<IDungeonEntity>();
            var size = characterMap.Size();
            var characters = characterMap.GetInXYZ();
            foreach (Vector3Int pos in VectorUtilities.IterateAllIn(size))
            {
                var character = characters[pos] ?? defaultChar;
                if (!entityFactories.TryGetValue(character, out var factory)) continue;
                var position = pos;
                var entity = factory(position);
                entities.Add(entity);
            }
            
            return entities;
        }

    }
}