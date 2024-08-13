using System.Collections.Generic;
using System.Linq;

namespace Dman.GridGameTools.EventLog
{
    public record ClearEventLogCommand : IDungeonCommand
    {
        public EntityId ActionTaker => EntityId.Invalid;
        public MovementExpectation ExpectsToCauseMovement => MovementExpectation.WillNotMove;
        
        public IEnumerable<IDungeonCommand> ApplyCommand(ICommandDungeon world)
        {
            var eventLog = world.WritableComponentStore.AssertGet<IEventLogWriter>();
            eventLog.FlushEventLog();
            return Enumerable.Empty<IDungeonCommand>();
        }
    }
}