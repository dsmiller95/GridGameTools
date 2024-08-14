using System.Collections.Generic;
using JetBrains.Annotations;

public enum MovementExpectation
{
    /// <summary>
    /// The -only- outcome of this command is movement of entities.
    /// </summary>
    OnlyMove,
    /// <summary>
    /// This command may cause movement of entities, and also may modify other properties of entities.
    /// </summary>
    MightMove,
    /// <summary>
    /// This command will not move any entities. but it may cause changes to other properties of entities.
    /// </summary>
    WillNotMove
}

public interface IDungeonCommand
{
    public IEnumerable<IDungeonCommand> ApplyCommand(ICommandDungeon world);
    [CanBeNull] public EntityId ActionTaker { get; }
    public MovementExpectation ExpectsToCauseMovement { get; }

    public IEnumerable<IDungeonCommand> ApplyCommandWithProfileSpans(ICommandDungeon world)
    {
        UnityEngine.Profiling.Profiler.BeginSample($"{this.GetType().Name} Command");
        var result = ApplyCommand(world);
        UnityEngine.Profiling.Profiler.EndSample();
        return result;
    }
}
