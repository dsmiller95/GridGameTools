using System.Collections.Generic;
using System.Linq;
using Dman.Math;
using GridRandom;
using UnityEngine;

public readonly struct DungeonBounds
{
    /// <summary>
    /// Minimum bounds, inclusive
    /// </summary>
    public readonly Vector3Int Min;
    /// <summary>
    /// Maximum bound, exclusive
    /// </summary>
    public readonly Vector3Int Max;

    public Vector3Int Size => Max - Min;
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="min">inclusive</param>
    /// <param name="max">exclusive</param>
    public DungeonBounds(Vector3Int min, Vector3Int max)
    {
        Max = max;
        Min = min;
    }
    
    public static DungeonBounds Including(params Vector3Int[] includes)
    {
        var bounds = new DungeonBounds(includes[0], includes[0]);
        foreach (Vector3Int include in includes.Skip(1))
        {
            bounds = bounds.Extend(include);
        }

        return bounds;
    }
    
    public bool Contains(Vector3Int position)
    {
        return position.x >= Min.x && position.x < Max.x &&
               position.y >= Min.y && position.y < Max.y &&
               position.z >= Min.z && position.z < Max.z;
    }

    public DungeonBounds Extend(Vector3Int point)
    {
        return new DungeonBounds(
            Min.MinComponents(point),
            Max.MaxComponents(point + Vector3Int.one)
        );
    }
}

public static class DungeonBoundsExtensions
{
    public static IEnumerable<Vector3Int> AllPoints(this DungeonBounds bounds)
    {
        return VectorUtilities.IterateAllIn(bounds.Min, bounds.Max);
    }

    public static Vector3Int PickRandomPont(this DungeonBounds bounds, ref GridRandomGen rng)
    {
        var size = bounds.Size;
        return new Vector3Int(
            rng.Next(size.x),
            rng.Next(size.y),
            rng.Next(size.z)
        ) + bounds.Min;
    }
}