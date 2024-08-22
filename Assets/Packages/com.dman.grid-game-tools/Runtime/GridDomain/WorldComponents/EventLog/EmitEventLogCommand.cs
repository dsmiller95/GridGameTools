using System.Collections.Generic;
using System.Linq;
using Dman.GridGameTools.Commands;
using Dman.GridGameTools.Entities;

namespace Dman.GridGameTools.EventLog
{
    public record EmitEventLogCommand(IGridEvent Evt) : IDungeonCommand
    {
        public EntityId ActionTaker => EntityId.Invalid;
        public MovementExpectation ExpectsToCauseMovement => MovementExpectation.WillNotMove;
        private IGridEvent Evt { get; } = Evt;

        public IEnumerable<IDungeonCommand> ApplyCommand(ICommandDungeon world)
        {
            var eventLog = world.WritableComponentStore.TryGet<IEventLogWriter>();
            if (eventLog?.AllowLog == true)
            {
                eventLog.LogEvent(Evt);
            }
            return Enumerable.Empty<IDungeonCommand>();
        }

        public void Deconstruct(out IGridEvent Evt)
        {
            Evt = this.Evt;
        }
    }
}