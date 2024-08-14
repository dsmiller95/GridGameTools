using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;

public record DungeonPathingData : IDungeonBakedPathingData
{
    public DungeonBounds Bounds { get; }
    private PooledArray3D<BlockedTileLayers> BlockedFaces { get; set; }
    public Vector3Int PathedToPosition { get; private set; }
    
    public DungeonPathingData(DungeonBounds bounds, Vector3Int playerPosition)
    {
        this.Bounds = bounds;
        BlockedFaces = PooledArray3D<BlockedTileLayers>.CreateAndFill(bounds.Size, BlockedTileLayers.Empty);
        PathedToPosition = playerPosition;
    }

    private DungeonPathingData(DungeonBounds bounds, Vector3Int playerPosition, PooledArray3D<BlockedTileLayers> blockedFaces)
    {
        if(blockedFaces.Size != bounds.Size)
            throw new ArgumentException("blockedFaces must match bounds");
        // if(blockedFaces.GetLength(0) != bounds.Size.x ||
        //    blockedFaces.GetLength(1) != bounds.Size.y ||
        //    blockedFaces.GetLength(2) != bounds.Size.z)
        //     throw new ArgumentException("blockedFaces must match bounds");
        this.Bounds = bounds;
        BlockedFaces = blockedFaces;
        PathedToPosition = playerPosition;
    }
    
    public BlockedTileLayers GetAllBlockedDataByTile(Vector3Int position)
    {
        if (!Bounds.Contains(position)) return BlockedTileLayers.FullyBlocked;
        position -= Bounds.Min;
        return BlockedFaces[position.x, position.y, position.z];
    }
    public FacingDirectionFlags GetBlockedFaces(Vector3Int position, PathingLayers layer)
    {
        if (!Bounds.Contains(position)) return FacingDirectionFlags.All;
        position -= Bounds.Min;
        var blockedFace = BlockedFaces[position.x, position.y, position.z];
        return blockedFace.GetBlockedFaces(layer);
    }

    public IDungeonPathingDataWriter CreateWriter()
    {
        return new DungeonPathingDataWriter(this);
    }
    
    public void Dispose()
    {
        BlockedFaces.Dispose();
    }

    private class DungeonPathingDataWriter : IDungeonPathingDataWriter
    {
        public DungeonBounds Bounds { get; }

        public Vector3Int PathedToPosition { get; private set; }

        private PooledArray3D<BlockedTileLayers> BlockedFaces { get; set; }
        public DungeonPathingDataWriter(DungeonPathingData copyFrom)
        {
            this.Bounds = copyFrom.Bounds;
            this.PathedToPosition = copyFrom.PathedToPosition;
            this.BlockedFaces = PooledArray3D<BlockedTileLayers>.Copy(copyFrom.BlockedFaces);
        }
        
        public void BlockFaces(Vector3Int position, FacingDirectionFlags flags, PathingLayers layers)
        {
            if (!Bounds.Contains(position)) return;
            position -= Bounds.Min;
            var currentFlags = BlockedFaces[position.x, position.y, position.z];
            currentFlags.BlockFaces(layers, flags);
            BlockedFaces[position.x, position.y, position.z] = currentFlags;
        }

        public void SetBlockedFaces(Vector3Int position, BlockedTileLayers blockedTileFull)
        {
            if (!Bounds.Contains(position)) return;
            position -= Bounds.Min;
            BlockedFaces[position.x, position.y, position.z] = blockedTileFull;
        }
        public void SetBlockedFaces(Vector3Int position, FacingDirectionFlags flags, PathingLayers layers)
        {
            if (!Bounds.Contains(position)) return;
            position -= Bounds.Min;
            var currentFlags = BlockedFaces[position.x, position.y, position.z];
            currentFlags.SetBlockedFaces(layers, flags);
            BlockedFaces[position.x, position.y, position.z] = currentFlags;
        }
        
        public void SetNewPlayerPosition(Vector3Int position)
        {
            if (PathedToPosition == position) return;
            PathedToPosition = position;
        }
        
        public BlockedTileLayers GetAllBlockedDataByTile(Vector3Int position)
        {
            if (!Bounds.Contains(position)) return BlockedTileLayers.FullyBlocked;
            position -= Bounds.Min;
            return BlockedFaces[position.x, position.y, position.z];
        }
        public FacingDirectionFlags GetBlockedFaces(Vector3Int position, PathingLayers layer)
        {
            if (!Bounds.Contains(position)) return FacingDirectionFlags.All;
            position -= Bounds.Min;
            var blockedFace = BlockedFaces[position.x, position.y, position.z];
            return blockedFace.GetBlockedFaces(layer);
        }
        
        public IDungeonPathingDataWriter CreateWriter()
        {
            throw new InvalidOperationException("created a writer to write to an existing writer");
            // Debug.LogWarning("created a writer to write to an existing writer");
            // return this.BuildAndDispose().CreateWriter();
        }
        
        private bool _isDisposed = false;
        public IDungeonBakedPathingData BakeImmutable(bool andDispose)
        {
            if(_isDisposed) throw new ObjectDisposedException("DungeonPathingDataWriter");
            PooledArray3D<BlockedTileLayers> facesToBuildWith;
            if (andDispose)
            {
                // mark as disposed, we are "releasing" the blocked faces memory, into the newly baked object
                _isDisposed = true;
                facesToBuildWith = BlockedFaces;
            }
            else
            {
                facesToBuildWith = PooledArray3D<BlockedTileLayers>.Copy(BlockedFaces);
            }

            // we always transfer our memory into the new built object. don't dispose.
            Profiler.BeginSample("DungeonPathingDataWriter.Build");
            var result = new DungeonPathingData(Bounds, PathedToPosition, facesToBuildWith);
            Profiler.EndSample();
            return result;
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;
            BlockedFaces.Dispose();
        }

        public void EntityChange(EntityWriteRecord writeRecord, IEntityStore upToDateStore)
        {
            if (writeRecord.NewEntity is IAmPathedTo)
            {
                Vector3Int currentPosition = this.PathedToPosition;
                if (writeRecord.NewEntity.Coordinate.Position != currentPosition)
                {
                    this.SetNewPlayerPosition(writeRecord.NewEntity.Coordinate.Position);
                }
            }
            
            if (writeRecord.OldEntity is IBlockTile)
            {
                Vector3Int position = writeRecord.OldEntity.Coordinate.Position;
                BlockedTileLayers newBlocking = upToDateStore.QueryFacesBlockedFrom(position);
                this.SetBlockedFaces(position, newBlocking);
            }

            if (writeRecord.NewEntity is IBlockTile)
            {
                Vector3Int position = writeRecord.NewEntity.Coordinate.Position;
                BlockedTileLayers newBlocking = upToDateStore.QueryFacesBlockedFrom(position);
                this.SetBlockedFaces(position, newBlocking);
            }
        }
    }
}