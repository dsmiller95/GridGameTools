using System;

[Flags]
public enum DungeonEntityFlags
{
    /// <summary>
    /// Set to true if it is possible for this entity to move.
    /// </summary>
    Mobile = 1<<0,
    MayDestroy = 1<<1,
    MayChangeState = 1<<2,
    
    NonStatic = Mobile | MayDestroy | MayChangeState,
    
    
    //GodMode= 1<<31,
    None = 0,
}