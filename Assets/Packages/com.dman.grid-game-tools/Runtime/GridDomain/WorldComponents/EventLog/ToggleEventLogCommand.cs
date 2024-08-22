using System.Collections.Generic;
using System.Linq;
using Dman.GridGameTools.Commands;
using Dman.GridGameTools.Entities;

namespace Dman.GridGameTools.EventLog
{
    public record ToggleEventLogCommand(bool AllowLogs) : IDungeonCommand
    {
        public EntityId ActionTaker => EntityId.Invalid;
        public MovementExpectation ExpectsToCauseMovement => MovementExpectation.WillNotMove;
        public bool AllowLogs { get; } = AllowLogs;

        public IEnumerable<IDungeonCommand> ApplyCommand(ICommandDungeon world)
        {
            var eventLog = world.WritableComponentStore.AssertGet<IEventLogWriter>();
            eventLog.SetAllowLog(AllowLogs);
            return Enumerable.Empty<IDungeonCommand>();
        }
    }
}