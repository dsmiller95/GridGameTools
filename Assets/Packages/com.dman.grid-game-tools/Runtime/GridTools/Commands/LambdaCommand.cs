using System;
using System.Collections.Generic;
using UnityEngine;

public class LambdaCommand : IDungeonCommand
{
    public EntityId ActionTaker => null;
    public Action<ICommandDungeon> Lambda { get; set; }
    public MovementExpectation ExpectsToCauseMovement => MovementExpectation.WillNotMove;
    public LambdaCommand(Action<ICommandDungeon> lambda)
    {
        Lambda = lambda;
    }
    
    public IEnumerable<IDungeonCommand> ApplyCommand(ICommandDungeon world)
    {
        Lambda(world);
        return Array.Empty<IDungeonCommand>();
    }
}