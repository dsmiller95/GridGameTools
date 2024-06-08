using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class DungeonWorldFactory
{ 
    public static IDungeonWorld CreateDungeonWorld(InitialDungeonState initialDungeonState)
    {
        var bounds = new DungeonBounds(initialDungeonState.minBounds, initialDungeonState.maxBounds);
        ulong seed = initialDungeonState.LongSeedOrRngIfDefault;
        return DungeonWorld.CreateEmpty(bounds, seed);
    }
}