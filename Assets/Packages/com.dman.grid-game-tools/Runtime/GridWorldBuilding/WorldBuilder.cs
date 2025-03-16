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

        public static WorldBuilder Create()
        {
            return new WorldBuilder()
                .AddComponents(DefaultComponents.Pathing)
                .RequiresComponent<IDungeonPathingData>();
        }

        private IDefaultFactoriesFactory DefaultEntities { get; set; }
        private string DefaultChar { get; set; } = "-";
        private uint Seed { get; set; } = 0;
        private WorldBuildConfig BuildConfig { get; set; } = WorldBuildConfig.Default;
        private IEnumerable<(string, Func<Vector3Int, IDungeonEntity>)> OtherFactories { get; set; } = Enumerable.Empty<(string, Func<Vector3Int, IDungeonEntity>)>();
        private string CharacterMap { get; set; } = null;
        private IEnumerable<IWorldComponent> Components { get; set; } = Enumerable.Empty<IWorldComponent>();
        private IEnumerable<ICreateDungeonComponent> ComponentCreators { get; set; } = Enumerable.Empty<ICreateDungeonComponent>();
        private IEnumerable<Type> RequiredComponents { get; set; } = Enumerable.Empty<Type>();
        private bool ShuffleEntities { get; set; } = false;
        
        /// <summary>
        /// Default Entities is used to generate the list of entity factories when the world is built. These associate characters to a function which generates an entity, given a position.
        /// </summary>
        /// <remarks>
        /// The generated factories can be overriden with AddOtherFactories
        /// </remarks>
        public WorldBuilder WithDefaultEntities(IDefaultFactoriesFactory defaultEntities) =>
            this with { DefaultEntities = defaultEntities };
        
        /// <summary>
        /// The default Char is used when interpreting the Map to indicate spaces which should not be expected to appear in the entity factories. 
        /// </summary>
        public WorldBuilder WithDefaultChar(string defaultChar) =>
            this with { DefaultChar = defaultChar };
        
        /// <summary>
        /// The seed is used when generating entities and passed in as the root seed of the created world.<br/>
        /// When set, the world will behave deterministically.<br/>
        /// When unset or set to 0, the seeds will be chosen via UnityEngine.Random when built.
        /// </summary>
        public WorldBuilder WithSeed(uint seed) =>
            this with { Seed = seed };
        
        /// <summary>
        /// Other entity factories add to or override the factories provided by the DefaultEntities factory.
        /// </summary>
        public WorldBuilder AddOtherFactories(params (string, Func<Vector3Int, IDungeonEntity>)[] otherFactories) =>
            otherFactories == null ? this : this with { OtherFactories = OtherFactories.Concat(otherFactories) };
        /// <summary>
        /// Other entity factories add to or override the factories provided by the DefaultEntities factory.
        /// </summary>
        public WorldBuilder AddOtherFactories(IEnumerable<(string, Func<Vector3Int, IDungeonEntity>)> otherFactories) =>
            otherFactories == null ? this : this with { OtherFactories = OtherFactories.Concat(otherFactories) };
        
        /// <summary>
        /// The build config defines how the character map string is interpreted by the builder. For example, if the Y-axis go down or up the string.
        /// </summary>
        public WorldBuilder WithBuildConfig(WorldBuildConfig buildConfig) =>
            this with { BuildConfig = buildConfig };
        
        /// <summary>
        /// The character map defines the layout of the world and the entities that will be created.
        /// </summary>
        public WorldBuilder WithMap(string characterMap) =>
            this with { CharacterMap = characterMap };
        /// <inheritdoc cref="WithMap(string)"/>
        public WorldBuilder WithMap(string characterMap, WorldBuildConfig buildConfig) =>
            this with { CharacterMap = characterMap, BuildConfig = buildConfig };
        /// <inheritdoc cref="WithMap(string)"/>
        public WorldBuilder WithMap(WorldBuildString characterMap) => 
            characterMap == null ? this : this with { CharacterMap = characterMap.OriginalCharacterMap, BuildConfig = characterMap.BuildConfig };
        
        
        /// <summary>
        /// Replaces components in the builder. These components will be added to the world when it is built.
        /// </summary>
        /// <remarks>
        /// Overrides any previously added components. If possible, prefer AddComponents instead.
        /// </remarks>
        public WorldBuilder WithComponents(IEnumerable<IWorldComponent> components) =>
            components == null ? this : this with { Components = components };
        /// <inheritdoc cref="AddComponents(IWorldComponent[])" />
        public WorldBuilder AddComponents(IEnumerable<IWorldComponent> components) =>
            components == null ? this : this with { Components = Components.Concat(components) };
        /// <summary>
        /// Adds components to the builder. These components will be added to the world when it is built.
        /// </summary>
        public WorldBuilder AddComponents(params IWorldComponent[] components) =>
            components == null ? this : this with { Components = Components.Concat(components) };
        /// <summary>
        /// Replaces component creators in the builder. These creators will be used to create components when the world is built.
        /// </summary>
        /// <remarks>
        /// Overrides any previously added component creators. If possible, prefer AddComponents instead.
        /// </remarks>
        public WorldBuilder WithComponents(IEnumerable<ICreateDungeonComponent> componentCreators) =>
            componentCreators == null ? this : this with { ComponentCreators = componentCreators };
        /// <inheritdoc cref="AddComponents(ICreateDungeonComponent[])" />
        public WorldBuilder AddComponents(IEnumerable<ICreateDungeonComponent> componentCreators) =>
            componentCreators == null ? this : this with { ComponentCreators = ComponentCreators.Concat(componentCreators) };
        /// <summary>
        /// Adds component creators to the builder. These creators will be used to create components when the world is built.
        /// </summary>
        public WorldBuilder AddComponents(params ICreateDungeonComponent[] componentCreators) =>
            componentCreators == null ? this : this with { ComponentCreators = ComponentCreators.Concat(componentCreators) };
        
        /// <inheritdoc cref="RequiresComponent{T}" />
        public WorldBuilder RequiresComponents(params Type[] requiredComponents) =>
            requiredComponents == null ? this : this with { RequiredComponents = RequiredComponents.Concat(requiredComponents) };
        /// <inheritdoc cref="RequiresComponent{T}" />
        public WorldBuilder RequiresComponents(IEnumerable<Type> requiredComponents) => 
            requiredComponents == null ? this : this with { RequiredComponents = RequiredComponents.Concat(requiredComponents) };
        /// <summary>
        /// When a component type is required, the builder will assert that a component implementing this type is present when built into a world.
        /// </summary>
        public WorldBuilder RequiresComponent<T>() => 
            this with { RequiredComponents = RequiredComponents.Append(typeof(T)) };
        
        /// <summary>
        /// When true, the entities created by the builder will be shuffled before being added to the world.
        /// This is useful to assert that logic is execution order independent, as execution order is determined by the ordering of entities.
        /// </summary>
        public WorldBuilder WithShuffleEntities(bool shuffleEntities) =>
            this with { ShuffleEntities = shuffleEntities };

        [Obsolete("Use Create() and then configure the builder with the fluent API instead.")]
        public static WorldBuilder Create(IDefaultFactoriesFactory defaultEntities, string defaultChar = "-", uint seed = 0, IEnumerable<(string, Func<Vector3Int, IDungeonEntity>)> otherFactories = null)
        {
            return Create()
                .WithDefaultEntities(defaultEntities)
                .WithDefaultChar(defaultChar)
                .WithSeed(seed)
                .AddOtherFactories(otherFactories);
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
            var creationContext = new WorldComponentCreationContext { WorldBounds = bounds };

            var allComponents = ComponentCreators
                .SelectMany(creator => creator.CreateComponents(creationContext))
                .Concat(Components)
                .ToList();
            
            // check for required components
            var missingComponents = GetMissingRequiredComponents(allComponents, RequiredComponents).ToList();
            if (missingComponents.Any())
            {
                throw new ArgumentException($"Missing required components: {string.Join(", ", missingComponents.Select(c => c.Name))}");
            }
            
            IDungeonWorld world = DungeonWorld.CreateEmpty(Seed, allComponents)
                .AddEntities(allEntities).world;

            return new WorldBuilderPatternResult(world, entityFactories, BuildConfig);
        }
        
        private static IEnumerable<Type> GetMissingRequiredComponents(
            IEnumerable<IWorldComponent> allComponents,
            IEnumerable<Type> requiredComponents)
        {
            var allComponentTypes = allComponents.Select(c => c.GetType()).ToList();
            foreach (Type requiredComponent in requiredComponents)
            {
                if (allComponentTypes.Any(x => requiredComponent.IsAssignableFrom(x)))
                {
                    continue;
                }
                yield return requiredComponent;
            }
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