using System;
using Dman.Math;
using UnityEngine;


public enum RelativeDirection
{
    Forward,
    Right,
    Backward,
    Left
}


public enum FacingDirection
{
    North,
    East,
    South,
    West,
}

[Flags]
public enum FacingDirectionFlags
{
    North = 1<<0,
    East = 1<<1,
    South = 1<<2,
    West = 1<<3,
    Up = 1<<4,
    Down = 1<<5,
    None = 0,
    All = North | East | South | West | Up | Down,
    
    Vertical = Up | Down,
    Horizontal = North | East | South | West,
    NorthSouth = North | South,
    EastWest = East | West,
}

public static class FacingDirectionExtensions
{
    public static readonly FacingDirectionFlags[] AllDirections = {
        FacingDirectionFlags.North,
        FacingDirectionFlags.East,
        FacingDirectionFlags.South,
        FacingDirectionFlags.West,
        FacingDirectionFlags.Up,
        FacingDirectionFlags.Down,
    };
    
    public static FacingDirectionFlags ToFlags(this FacingDirection direction)
    {
        return direction switch
        {
            FacingDirection.North => FacingDirectionFlags.North,
            FacingDirection.East => FacingDirectionFlags.East,
            FacingDirection.South => FacingDirectionFlags.South,
            FacingDirection.West => FacingDirectionFlags.West,
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
        };
    }
    
    public static FacingDirection? TryFromFlags(this FacingDirectionFlags flags)
    {
        if (flags.HasFlag(FacingDirectionFlags.North)) return FacingDirection.North;
        if (flags.HasFlag(FacingDirectionFlags.East)) return FacingDirection.East;
        if (flags.HasFlag(FacingDirectionFlags.South)) return FacingDirection.South;
        if (flags.HasFlag(FacingDirectionFlags.West)) return FacingDirection.West;
        return null;
    }

    public static FacingDirectionFlags ToFacingFlags(this Vector3Int direction)
    {
        var result = FacingDirectionFlags.None;
        
        if (direction.x != 0)
        {
            result |= direction.x > 0 ? FacingDirectionFlags.East : FacingDirectionFlags.West;
        }
        if (direction.y != 0)
        {
            result |= direction.y > 0 ? FacingDirectionFlags.Up : FacingDirectionFlags.Down;
        }
        if (direction.z != 0)
        {
            result |= direction.z > 0 ? FacingDirectionFlags.North : FacingDirectionFlags.South;
        }

        return result;
    }

    public static FacingDirectionFlags ToFacing(this Vector3Int direction)
    {
        if(direction.GetNonzeroAxisCount() != 1)
        {
            throw new ArgumentException("Direction must be one-dimensional");
        }
        if (direction.x != 0)
        {
            return direction.x > 0 ? FacingDirectionFlags.East : FacingDirectionFlags.West;
        }
        if (direction.y != 0)
        {
            return direction.y > 0 ? FacingDirectionFlags.Up : FacingDirectionFlags.Down;
        }
        if (direction.z != 0)
        {
            return direction.z > 0 ? FacingDirectionFlags.North : FacingDirectionFlags.South;
        }
        return FacingDirectionFlags.None;
    }

    public static FacingDirection? ToBestFacingOption(this Vector3Int direction)
    {
        var absX = Math.Abs(direction.x);
        var absZ = Math.Abs(direction.z);
        if (absX > absZ)
        {
            return direction.x > 0 ? FacingDirection.East : FacingDirection.West;
        }

        if (absZ > absX)
        {
            return direction.z > 0 ? FacingDirection.North : FacingDirection.South;
        }

        return null;
    }
    
    public static FacingDirection? ToBestFacingAdjacentOnly(this Vector3Int direction)
    {
        if (direction.GetNonzeroAxisCount() != 1) return null;
        if(direction.x != 0)
        {
            return direction.x > 0 ? FacingDirection.East : FacingDirection.West;
        }
        if(direction.z != 0)
        {
            return direction.z > 0 ? FacingDirection.North : FacingDirection.South;
        }
        return null;
    }

    public static FacingDirectionFlags? ToBestFacingOptionIncludeUp(this Vector3Int direction)
    {
        var absX = Math.Abs(direction.x);
        var absY = Math.Abs(direction.y);
        var absZ = Math.Abs(direction.z);
        var max = Mathf.Max(absX, Mathf.Max(absY, absZ));
        if(max == 0)
        {
            return null;
        }
        
        if (absY == max)
        {
            return direction.y > 0 ? FacingDirectionFlags.Up : FacingDirectionFlags.Down;
        }
        if (absX == max)
        {
            return direction.x > 0 ? FacingDirectionFlags.East : FacingDirectionFlags.West;
        }
        if (absZ == max)
        {
            return direction.z > 0 ? FacingDirectionFlags.North : FacingDirectionFlags.South;
        }

        return null;
    }
    
    public static Vector3Int ToVectorDirection(this FacingDirection direction)
    {
        return direction switch
        {
            FacingDirection.North => new Vector3Int(0, 0, 1),
            FacingDirection.East => new Vector3Int(1, 0, 0),
            FacingDirection.South => new Vector3Int(0, 0, -1),
            FacingDirection.West => new Vector3Int(-1, 0, 0),
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
        };
    }
    public static Vector3Int ToVectorDirection(this FacingDirectionFlags direction)
    {
        return direction switch
        {
            FacingDirectionFlags.North => new Vector3Int(0, 0, 1),
            FacingDirectionFlags.East => new Vector3Int(1, 0, 0),
            FacingDirectionFlags.South => new Vector3Int(0, 0, -1),
            FacingDirectionFlags.West => new Vector3Int(-1, 0, 0),
            FacingDirectionFlags.Up => new Vector3Int(0, 1, 0),
            FacingDirectionFlags.Down => new Vector3Int(0, -1, 0),
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
        };
    }

    
    public static FacingDirectionFlags Transform(this FacingDirection direction, FacingDirectionFlags flags)
    {
        var verticalOnly = (FacingDirectionFlags.Up | FacingDirectionFlags.Down);
        var verticality = flags & verticalOnly;
        var horizontal = (int)(flags & ~verticalOnly);
        
        var rotatedHorizontal = (FacingDirectionFlags)Rot4(horizontal, (int)direction);

        return verticality | rotatedHorizontal;
    }
    
    private static int Rot4(int input, int shift)
    {
        shift %= 4;
        var shifted = ((int)input) << shift;
        var nonOverflow = shifted & 0b00001111;
        var overflow    = shifted & 0b11110000;
        return nonOverflow | (overflow >> 4);
    }
    
    public static FacingDirection Transform(this FacingDirection direction, RelativeDirection relative)
    {
        var transformed = (int)direction + (int)relative;
        return (FacingDirection)(transformed % 4);
    }
    public static RelativeDirection Transform(this RelativeDirection direction, RelativeDirection relative)
    {
        var transformed = (int)direction + (int)relative;
        return (RelativeDirection)(transformed % 4);
    }

    public static RelativeDirection Inverse(this RelativeDirection direction)
    {
        return (RelativeDirection)(((int)direction + 2) % 4);
    }

    public static Vector3Int GetRelativeDirectionFromAbsolute(this FacingDirection direction, Vector3Int absoluteDirection)
    {
        Quaternion rotation = direction switch
        {
            FacingDirection.North => Quaternion.Euler(0, 0, 0),
            FacingDirection.East => Quaternion.Euler(0, 90, 0),
            FacingDirection.South => Quaternion.Euler(0, 180, 0),
            FacingDirection.West => Quaternion.Euler(0, 270, 0),
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
        };
        Vector3 rotated = Quaternion.Inverse(rotation) * absoluteDirection;
        return new Vector3Int(Mathf.RoundToInt(rotated.x), Mathf.RoundToInt(rotated.y), Mathf.RoundToInt(rotated.z));
    }
    
    public static Vector3Int GetAbsoluteDirectionFromRelative(this FacingDirection direction, Vector3Int relativeDirection)
    {
        Quaternion rotation = direction switch
        {
            FacingDirection.North => Quaternion.Euler(0, 0, 0),
            FacingDirection.East => Quaternion.Euler(0, 90, 0),
            FacingDirection.South => Quaternion.Euler(0, 180, 0),
            FacingDirection.West => Quaternion.Euler(0, 270, 0),
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
        };
        Vector3 rotated = rotation * relativeDirection;
        return new Vector3Int(Mathf.RoundToInt(rotated.x), Mathf.RoundToInt(rotated.y), Mathf.RoundToInt(rotated.z));
    }
    
    public static FacingDirection Turn(this FacingDirection direction, TurnDirection turnDirection)
    {
        var delta = turnDirection switch
        {
            TurnDirection.Left => -1,
            TurnDirection.Right => 1,
            _ => throw new ArgumentOutOfRangeException()
        };
        return (FacingDirection)(((int)direction + delta + 4) % 4);
    }
    
    
}