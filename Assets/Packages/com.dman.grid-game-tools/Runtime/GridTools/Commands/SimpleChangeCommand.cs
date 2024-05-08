using System;
using System.Collections.Generic;
using UnityEngine;

public class SimpleChangeCommand<T> : IDungeonCommand
{
    public EntityId ActionTaker { get; }
    public SimpleChangeCommand(EntityId actionTaker, Func<T, IDungeonEntity> lambda)
    {
        ActionTaker = actionTaker;
        Lambda = lambda;
    }

    public Func<T, IDungeonEntity> Lambda { get; set; }

    public IEnumerable<IDungeonCommand> ApplyCommand(ICommandDungeon world)
    {
        if (world.GetEntity(ActionTaker) is not T ofType)
        {
            Debug.LogWarning($"Entity {ActionTaker} is not of type {typeof(T)}");
            return Array.Empty<IDungeonCommand>();
        }
        
        world.SetEntity(ActionTaker,  Lambda(ofType));
        return Array.Empty<IDungeonCommand>();
    }

    public MovementExpectation ExpectsToCauseMovement => MovementExpectation.WillNotMove;
}