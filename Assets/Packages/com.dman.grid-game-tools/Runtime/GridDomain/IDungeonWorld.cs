using System;
using System.Collections.Generic;
using Dman.GridGameTools;

/// <summary>
/// A read only state of the world. can create new world states via Copy-On-Write mechanisms.
/// </summary>
public interface IDungeonWorld: IDisposable
{
    /// <summary>
    /// a constant seed for this world. never changes.
    /// </summary>
    public uint WorldRngSeed { get; }

    public DungeonBounds Bounds => PathingData.Bounds;

    public (IDungeonWorld newWorld, IEnumerable<IDungeonCommand> executedCommands)
        ApplyCommandsWithModifiedCommands(IEnumerable<IDungeonCommand> commands);

    public IDungeonPathingDataBaked PathingData => this.Components.AssertGet<IDungeonPathingDataBaked>();

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
