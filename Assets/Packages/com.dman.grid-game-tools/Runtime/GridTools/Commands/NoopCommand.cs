
using System;
using System.Collections.Generic;

public class NoopCommand: IDungeonCommand
{
    public EntityId ActionTaker => null;
    public MovementExpectation ExpectsToCauseMovement => MovementExpectation.WillNotMove;
    public IEnumerable<IDungeonCommand> ApplyCommand(ICommandDungeon world)
    {
        return Array.Empty<IDungeonCommand>();
    }
}
