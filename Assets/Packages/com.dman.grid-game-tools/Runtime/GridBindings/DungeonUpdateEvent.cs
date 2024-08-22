using System;
using System.Collections.Generic;
using Dman.GridGameTools;
using Dman.GridGameTools.Commands;
using JetBrains.Annotations;

namespace Dman.GridGameBindings
{
    public record DungeonUpdateEvent(IDungeonWorld OldWorld, IDungeonWorld NewWorld, IReadOnlyList<IDungeonCommand> AppliedCommands = null)
    {
        public IDungeonWorld NewWorld { get; } = NewWorld;
        /// <summary>
        /// When null, is a world creation event
        /// </summary>
        [CanBeNull] public IDungeonWorld OldWorld { get; } = OldWorld;

        public IReadOnlyList<IDungeonCommand> AppliedCommands { get; } = AppliedCommands ?? Array.Empty<IDungeonCommand>();
    }
}