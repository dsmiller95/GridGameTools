using System;
using System.Collections.Generic;

/// <summary>
/// A read only state of the world. can create new world states via Copy-On-Write mechanisms.
/// </summary>
public interface IDungeonWorld: IDisposable
{
    /// <summary>
    /// a constant seed for this world. never changes.
    /// </summary>
    public ulong WorldRngSeed { get; }
    public DungeonBounds Bounds { get; }

    public (IDungeonWorld newWorld, IEnumerable<IDungeonCommand> executedCommands)
        ApplyCommandsWithModifiedCommands(IEnumerable<IDungeonCommand> commands);
    public IDungeonBakedPathingData PathingData { get; }

    public ICachingEntityStore EntityStore { get; }
    public IComponentStore Components { get; }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="commands"></param>
    /// <param name="andDispose">When true, will dispose this world after the next world is created</param>
    /// <returns></returns>
    public (IDungeonWorld newWorld, IEnumerable<IDungeonCommand> executedCommands) ApplyCommandsWithModifiedCommands(
        IEnumerable<IDungeonCommand> commands,
        bool andDispose)
    {
        var result = ApplyCommandsWithModifiedCommands(commands);
        if (andDispose)
        {
            Dispose();
        }
        return result;
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="commands"></param>
    /// <param name="andDispose">When true, will dispose this world after the next world is created</param>
    /// <returns></returns>
    public IDungeonWorld ApplyCommands(IEnumerable<IDungeonCommand> commands, bool andDispose = false)
    {
        return ApplyCommandsWithModifiedCommands(commands, andDispose).newWorld;
    }
}
