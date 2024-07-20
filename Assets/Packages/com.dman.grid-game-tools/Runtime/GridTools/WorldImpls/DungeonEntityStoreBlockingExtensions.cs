using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;

public static class DungeonEntityStoreBlockingExtensions
{
    public static BlockedTileLayers QueryFacesBlockedFrom(this IEntityStore entities, Vector3Int position)
    {
        var allEntities = entities.GetEntitiesAt(position);
        var blocked = BlockedTileLayers.Empty;
        foreach (var entityId in allEntities)
        {
            var entity = entities.GetEntity(entityId);
            if (entity is IBlockTile blocking)
            {
                var blockedRelative = blocking.BlockingDirections;
                var blockedAbsolute = entity.Coordinate.FacingDirection.Transform(blockedRelative);
                blocked.BlockFaces(blocking.BlockingLayers, blockedAbsolute);
            }
        }

        return blocked;
    }

    public static void ApplyWrite(
        this IDungeonPathingDataWriter writer,
        EntityWriteRecord writeRecord,
        IEntityStore upToDateStore)
    {
        if (writeRecord.NewEntity is IAmPathedTo)
        {
            Vector3Int currentPosition = writer.PathedToPosition;
            if (writeRecord.NewEntity.Coordinate.Position != currentPosition)
            {
                writer.SetNewPlayerPosition(writeRecord.NewEntity.Coordinate.Position);
            }
        }
;
        if (writeRecord.OldEntity is IBlockTile)
        {
            Vector3Int position = writeRecord.OldEntity.Coordinate.Position;
            BlockedTileLayers newBlocking = upToDateStore.QueryFacesBlockedFrom(position);
            writer.SetBlockedFaces(position, newBlocking);
        }

        if (writeRecord.NewEntity is IBlockTile)
        {
            Vector3Int position = writeRecord.NewEntity.Coordinate.Position;
            BlockedTileLayers newBlocking = upToDateStore.QueryFacesBlockedFrom(position);
            writer.SetBlockedFaces(position, newBlocking);
        }
    }
    
    public static IDungeonBakedPathingData ApplyWriteRecord(
        this IDungeonPathingData existingPathing,
        IEntityStore newStore,
        IEnumerable<EntityWriteRecord> writeRecords)
    {
        Profiler.BeginSample("DungeonEntityStoreBlockingExtensions.ApplyWriteRecord");
        var positionsToCheck = new HashSet<Vector3Int>();
        Vector3Int? mostRecentPlayerPosition = null;
        foreach (var writeRecord in writeRecords)
        {
            if (writeRecord.NewEntity is IAmPathedTo)
            {
                mostRecentPlayerPosition = writeRecord.NewEntity.Coordinate.Position;
            }
            switch (writeRecord.ChangeType)
            {
                case EntityWriteRecord.Change.Add:
                    if(writeRecord.NewEntity is IBlockTile)
                    {
                        positionsToCheck.Add(writeRecord.NewEntity.Coordinate.Position);
                    }
                    break;
                case EntityWriteRecord.Change.Delete:
                    if(writeRecord.OldEntity is IBlockTile)
                    {
                        positionsToCheck.Add(writeRecord.OldEntity.Coordinate.Position);
                    }

                    break;
                case EntityWriteRecord.Change.Update:
                    if(writeRecord.OldEntity is IBlockTile)
                    {
                        positionsToCheck.Add(writeRecord.OldEntity.Coordinate.Position);
                    }
                    if(writeRecord.NewEntity is IBlockTile)
                    {
                        positionsToCheck.Add(writeRecord.NewEntity.Coordinate.Position);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        using var newPathingData = existingPathing.CreateWriter();
        foreach (var position in positionsToCheck)
        {
            var blockedFaces = existingPathing.GetAllBlockedDataByTile(position);
            var newBlockedFaces = newStore.QueryFacesBlockedFrom(position);
            if (blockedFaces != newBlockedFaces)
            {
                newPathingData.SetBlockedFaces(position, newBlockedFaces);
            }
        }
        var existingPlayerPosition = existingPathing.PathedToPosition;
        if(mostRecentPlayerPosition.HasValue && mostRecentPlayerPosition.Value != existingPlayerPosition)
        {
            newPathingData.SetNewPlayerPosition(mostRecentPlayerPosition.Value);
        }
        
        var result = newPathingData.BuildAndDispose();
        
        Profiler.EndSample();

        return result;
    }

    // public static IDungeonBakedPathingData ResetFullPathing(this IDungeonPathingData existingPathing, IEntityStore newStore)
    // {
    //     using var writer = existingPathing.CreateWriter();
    //     foreach (Vector3Int point in existingPathing.Bounds.AllPoints())
    //     {
    //         writer.SetBlockedFaces(point, FacingDirectionFlags.None);
    //     }
    //
    //     foreach (var (_, entity) in newStore.AllEntitiesWithIds())
    //     {
    //         if(entity is not IBlockTile blocking) continue;
    //         var blockedRelative = blocking.BlockingDirections;
    //         var blockedAbsolute = entity.Coordinate.FacingDirection.Transform(blockedRelative);
    //         writer.BlockFaces(entity.Coordinate.Position, blockedAbsolute, blocking.BlockingLayers);
    //     }
    //     
    //     return writer.BuildAndDispose();
    // }
}