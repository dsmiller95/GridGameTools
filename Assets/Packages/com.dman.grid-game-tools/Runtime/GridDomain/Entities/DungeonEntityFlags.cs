using System;

[Flags]
public enum DungeonEntityFlags
{
    /// <summary>
    /// Set to true if it is possible for this entity to move.
    /// </summary>
    Mobile = 1<<0,
    /// <summary>
    /// Set to true if it is possible for this entity to be destroyed.
    /// </summary>
    MayDestroy = 1<<1,
    /// <summary>
    /// Set to true if it is possible for this entity to change state.
    /// </summary>
    MayChangeState = 1<<2,
    
    //GodMode= 1<<31,
    None = 0,
    
    /// <summary>
    /// The entity may change in some way as the world changes
    /// </summary>
    NonStatic = Mobile | MayDestroy | MayChangeState,
}

public static class DungeonEntityFlagsExtensions
{
    public static bool IsStatic(this DungeonEntityFlags flags)
    {
        return (flags & DungeonEntityFlags.NonStatic) == 0;
    }
}