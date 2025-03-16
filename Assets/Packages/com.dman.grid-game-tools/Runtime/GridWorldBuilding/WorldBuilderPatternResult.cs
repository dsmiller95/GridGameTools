using System;
using System.Collections.Generic;
using Dman.GridGameTools.Entities;
using UnityEngine;

namespace Dman.GridGameTools.WorldBuilding
{
    public class WorldBuilderPatternResult
    {
        public IDungeonWorld World { get; set; }
        public IReadOnlyDictionary<string, Func<Vector3Int, IDungeonEntity>> FinalEntityFactories { get; set; }
        public WorldBuildConfig WorldBuildConfig { get; set; }
        
        internal WorldBuilderPatternResult(
            IDungeonWorld world,
            IReadOnlyDictionary<string, Func<Vector3Int, IDungeonEntity>> finalEntityFactories,
            WorldBuildConfig worldBuildConfig)
        {
            World = world;
            FinalEntityFactories = finalEntityFactories;
            WorldBuildConfig = worldBuildConfig;
        }
    }
}