using System;
using GridRandom;
using UnityEngine;

public readonly struct DungeonCoordinate : IEquatable<DungeonCoordinate>
{
    public readonly Vector3Int Position;
    public readonly FacingDirection FacingDirection;

    public DungeonCoordinate(Vector3Int position, FacingDirection facingDirection)
    {
        Position = position;
        FacingDirection = facingDirection;
    }

    public override string ToString()
    {
        return $"[{Position} {FacingDirection}]";
    }
    
    public Vector3Int GetPositionInFront()
    {
        return Position + FacingDirection.ToVectorDirection();
    }

    public DungeonCoordinate AdvanceForward(int steps)
    {
        return new DungeonCoordinate(
            Position + FacingDirection.ToVectorDirection() * steps,
            FacingDirection);
    }

    public bool Equals(DungeonCoordinate other)
    {
        return Position.Equals(other.Position) && FacingDirection == other.FacingDirection;
    }

    public override bool Equals(object obj)
    {
        return obj is DungeonCoordinate other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Position, (int)FacingDirection);
    }
     
    public static bool operator ==(DungeonCoordinate left, DungeonCoordinate right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(DungeonCoordinate left, DungeonCoordinate right)
    {
        return !(left == right);
    }
}

public static class DungeonCoordinateExtensions
{
    public static GridRandomGen Fork(this GridRandomGen randGen, DungeonCoordinate coordinate)
    {
        var seededToPosition = new GridRandomGen(coordinate.Position.GetHashCode());
        return GridRandomGen.Combine(randGen, seededToPosition);
    }
}