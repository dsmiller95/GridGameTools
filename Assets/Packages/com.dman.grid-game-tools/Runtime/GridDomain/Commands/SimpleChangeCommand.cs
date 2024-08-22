using System;
using System.Collections.Generic;
using Dman.GridGameTools.Entities;
using UnityEngine;

namespace Dman.GridGameTools.Commands
{
    public class SimpleChangeCommand<T> : IDungeonCommand
    {
        private readonly LogType? _logIfNotOfType;
        public EntityId ActionTaker { get; }
        public SimpleChangeCommand(EntityId actionTaker, Func<T, IDungeonEntity> lambda, LogType? logIfNotOfType = LogType.Warning)
        {
            _logIfNotOfType = logIfNotOfType;
            ActionTaker = actionTaker;
            Lambda = lambda;
        }

        public Func<T, IDungeonEntity> Lambda { get; set; }

        public IEnumerable<IDungeonCommand> ApplyCommand(ICommandDungeon world)
        {
            if (world.GetEntity(ActionTaker) is not T ofType)
            {
                if (_logIfNotOfType != null)
                {
                    Debug.unityLogger.Log(_logIfNotOfType.Value,$"Entity {ActionTaker} is not of type {typeof(T)}");
                }
                return Array.Empty<IDungeonCommand>();
            }
        
            world.SetEntity(ActionTaker,  Lambda(ofType));
            return Array.Empty<IDungeonCommand>();
        }

        public MovementExpectation ExpectsToCauseMovement => MovementExpectation.WillNotMove;
    }
}