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
    
    public IDungeonWorld ApplyCommands(IEnumerable<IDungeonCommand> commands, bool andDispose = false)
    {
        return ApplyCommandsWithModifiedCommands(commands, andDispose).newWorld;
    }
    
    public IDungeonBakedPathingData PathingData { get; }

    public ICachingEntityStore EntityStore { get; }
}
