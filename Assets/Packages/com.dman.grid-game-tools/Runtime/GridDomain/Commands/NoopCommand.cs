using System;
using System.Collections.Generic;
using Dman.GridGameTools.Entities;

namespace Dman.GridGameTools.Commands
{
    public class NoopCommand: IDungeonCommand
    {
        public EntityId ActionTaker => null;
        public MovementExpectation ExpectsToCauseMovement => MovementExpectation.WillNotMove;
        public IEnumerable<IDungeonCommand> ApplyCommand(ICommandDungeon world)
        {
            return Array.Empty<IDungeonCommand>();
        }
    }
}
