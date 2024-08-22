using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class DungeonWorldFactory
{ 
    public static IDungeonWorld CreateDungeonWorld(InitialDungeonState initialDungeonState)
    {
        var bounds = new DungeonBounds(initialDungeonState.minBounds, initialDungeonState.maxBounds);
        uint seed = initialDungeonState.SeedOrRngIfDefault;
        var pathingData = new DungeonPathingData(bounds, playerPosition: Vector3Int.zero);
        return DungeonWorld.CreateEmpty(seed, pathingData);
    }
}