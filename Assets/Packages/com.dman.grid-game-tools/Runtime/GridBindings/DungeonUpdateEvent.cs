using System;
using System.Collections.Generic;
using JetBrains.Annotations;

public record DungeonUpdateEvent(IDungeonWorld OldWorld, IDungeonWorld NewWorld, IReadOnlyList<IDungeonCommand> AppliedCommands = null)
{
    public IDungeonWorld NewWorld { get; } = NewWorld;
    /// <summary>
    /// When null, is a world creation event
    /// </summary>
    [CanBeNull] public IDungeonWorld OldWorld { get; } = OldWorld;

    public IReadOnlyList<IDungeonCommand> AppliedCommands { get; } = AppliedCommands ?? Array.Empty<IDungeonCommand>();
}