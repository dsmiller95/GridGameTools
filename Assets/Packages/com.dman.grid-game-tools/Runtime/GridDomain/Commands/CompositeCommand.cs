using System;
using System.Collections.Generic;
using System.Linq;
using Dman.GridGameTools.Entities;
using UnityEngine;

namespace Dman.GridGameTools.Commands
{
    public class CompositeCommand : IDungeonCommand
    {
        public List<IDungeonCommand> Commands { get; }
        public EntityId ActionTaker { get; }
        public MovementExpectation ExpectsToCauseMovement => MovementExpectation.MightMove;
        
        
        public CompositeCommand(List<IDungeonCommand> commands)
        {
            if (commands.Count <= 0) throw new InvalidOperationException("do not construct a composite command with no commands. instead, use NoopCommand");
            Commands = commands;
            var allIds = commands.Select(x => x.ActionTaker).Distinct().ToList();
            if (allIds.Count > 1)
            {
                Debug.LogWarning($"Found more than one id in a composite command. using null id. found ids: {string.Join(", ", allIds)}");
                ActionTaker = null;
            }
            else
            {
                ActionTaker = allIds.Single();
            }
        }
        
        public IEnumerable<IDungeonCommand> ApplyCommand(ICommandDungeon world) => Commands;
    }
}