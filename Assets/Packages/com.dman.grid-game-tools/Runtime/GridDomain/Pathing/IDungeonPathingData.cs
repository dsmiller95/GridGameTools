using System;
using System.Collections.Generic;
using GridRandom;
using UnityEngine;
using UnityEngine.Profiling;


public interface IDungeonPathfinding
{
    public IEnumerable<Vector3Int> PathToPathCenter(
        Vector3Int origin,
        GridRandomGen rngSeed,
        PathingLayers externalLayer = PathingLayers.All,
        PathingLayers internalLayer = PathingLayers.None);
}

public interface IDungeonBakedPathingData : IDungeonPathingData
{
    
}

public interface IDungeonPathingData: IDisposable
{
    public DungeonBounds Bounds { get; }
    public FacingDirectionFlags GetBlockedFaces(Vector3Int position, PathingLayers layer);

    public FacingDirectionFlags GetFacesBlockedBiDirectional(Vector3Int blockingSource, PathingLayers layer)
    {
        var allDirections = FacingDirectionExtensions.AllDirections;
        var allDirectionsOpposite = FacingDirectionExtensions.AllDirections;
        FacingDirectionFlags blockedFaces = FacingDirectionFlags.None;
        for (int i = 0; i < allDirections.Length; i++)
        {
            var direction = allDirections[i];
            var nextCell = blockingSource + direction.ToVectorDirection();
            var blockedBetween = IsBlockedBetween(blockingSource, nextCell, layer, layer);
            if (blockedBetween)
            {
                blockedFaces |= direction;
            }
        }

        return blockedFaces;
    }

    public BlockedTileLayers GetAllBlockedDataByTile(Vector3Int blockingSource);
    
    public bool IsBlocked(
        Vector3Int startAt,
        FacingDirection inDirection,
        PathingLayers layer,
        bool checkExternalCell = true)
    {
        if (checkExternalCell)
        {
            var nextTile = startAt + inDirection.ToVectorDirection();
            var blockedFaces = GetBlockedFaces(nextTile, layer);
            var backwardsDirection = inDirection.Transform(RelativeDirection.Backward);
            return blockedFaces.HasFlag(backwardsDirection.ToFlags());
        }
        else
        {
            var blockedFaces = GetBlockedFaces(startAt, layer);
            return blockedFaces.HasFlag(inDirection.ToFlags());
        }
    }

    public bool IsBlocked(
        Vector3Int startAt,
        FacingDirection inDirection,
        PathingLayers externalLayers,
        PathingLayers internalLayers)
    {
        {
            var blockedInternalFaces = GetBlockedFaces(startAt, internalLayers);
            var blockedInternal = blockedInternalFaces.HasFlag(inDirection.ToFlags());
            if (blockedInternal) return true;
        }
        
        {
            var nextTile = startAt + inDirection.ToVectorDirection();
            var blockedExternal = GetBlockedFaces(nextTile, externalLayers);
            var backwardsDirection = inDirection.Transform(RelativeDirection.Backward);
            return blockedExternal.HasFlag(backwardsDirection.ToFlags());
        }
    }
    
    public bool IsBlockedBetween(Vector3Int startAt, Vector3Int endAt, PathingLayers layer)
    {
        var delta = endAt - startAt;
        if (delta.sqrMagnitude != 1) throw new Exception("Cannot check for blockage across more than one tile");
        var direction = delta.ToFacing();
        var backwardsDirection = (-delta).ToFacing();
        var nextTile = startAt + direction.ToVectorDirection();
        var blockedFaces = GetBlockedFaces(nextTile, layer);
        return blockedFaces.HasFlag(backwardsDirection);
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="startAt">The voxel to move from</param>
    /// <param name="endAt">the voxel to move into</param>
    /// <param name="externalLayer">the layers which may block movement, inside the target voxel</param>
    /// <param name="internalLayer">the layers which may block movement, inside the source voxel</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public bool IsBlockedBetween(
        Vector3Int startAt,
        Vector3Int endAt,
        PathingLayers externalLayer,
        PathingLayers internalLayer)
    {
        var delta = endAt - startAt;
        if (delta.sqrMagnitude != 1) throw new Exception("Cannot check for blockage across more than one tile");
        var direction = delta.ToFacing();
        
        var blockedInternalFaces = GetBlockedFaces(startAt, internalLayer);
        var blockedInternal = blockedInternalFaces.HasFlag(direction);
        if (blockedInternal) return true;
        
        var backwardsDirection = (-delta).ToFacing();
        var nextTile = startAt + direction.ToVectorDirection();
        var blockedExternalFaces = GetBlockedFaces(nextTile, externalLayer);
        return blockedExternalFaces.HasFlag(backwardsDirection);
    }

    public bool IsBlockedCast(DungeonCoordinate castFrom, 
        int castLength,
        PathingLayers externalLayer,
        PathingLayers internalLayer)
    {
        var unit = castFrom.FacingDirection.ToVectorDirection();
        
        for (int i = 0; i < castLength; i++)
        {
            var from = castFrom.Position + unit * i;
            var to = from + unit;
            if (IsBlockedBetween(from, to, externalLayer, internalLayer))
            {
                return true;
            }
        }

        return false;
    }
    
    public Vector3Int PathedToPosition { get; }
    public IDungeonPathingDataWriter CreateWriter();
    
    public bool PropertiesEqual(IDungeonPathingData other)
    {
        if (!Equals(this.Bounds, other.Bounds)) return false;
        if (this.PathedToPosition != other.PathedToPosition) return false;
        
        Profiler.BeginSample("IDungeonPathingData_PropertiesEqual");
        
        foreach (var pointInside in this.Bounds.AllPoints())
        {
            if (this.GetAllBlockedDataByTile(pointInside) != other.GetAllBlockedDataByTile(pointInside))
            {
                Profiler.EndSample();
                return false;
            }
        }

        Profiler.EndSample();
        return true;
    }
}

/// <summary>
/// A writer to dungeon pathing data. Will dispose when built to a baked pathing data.
/// </summary>
public interface IDungeonPathingDataWriter : IDungeonPathingData
{
    /// <summary>
    /// Block the specified faces. additive. will not make anything passable, will only restrict movement
    /// </summary>
    /// <param name="position"></param>
    /// <param name="flags"></param>
    /// <param name="layers"></param>
    public void BlockFaces(Vector3Int position, FacingDirectionFlags flags, PathingLayers layers = PathingLayers.All);

    /// <summary>
    /// Set the faces which are blocked by the given position. may make certain edges passable.
    /// </summary>
    /// <param name="position"></param>
    /// <param name="flags"></param>
    /// <param name="layer"></param>
    public void SetBlockedFaces(Vector3Int position, FacingDirectionFlags flags, PathingLayers layer = PathingLayers.All);
    public void SetBlockedFaces(Vector3Int position, BlockedTileLayers blockedTileFull);
    public void SetNewPlayerPosition(Vector3Int position);
    /// <summary>
    /// When possible, prefer setting andDispose to true. this will transfer the memory of the writer into the baked object.
    /// This means 0 allocations or copies when andDispose is true.
    /// </summary>
    /// <param name="andDispose"></param>
    /// <returns></returns>
    public IDungeonBakedPathingData BakeImmutable(bool andDispose = true);

    public void BuildAndDisposeAndSwap(ref IDungeonPathingData other)
    {
        other?.Dispose();
        other = BakeImmutable();
    }
}