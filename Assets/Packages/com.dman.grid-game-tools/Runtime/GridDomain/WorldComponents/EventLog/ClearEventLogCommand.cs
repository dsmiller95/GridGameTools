﻿using System.Collections.Generic;
using System.Linq;
using Dman.GridGameTools.Commands;
using Dman.GridGameTools.Entities;

namespace Dman.GridGameTools.EventLog
{
    public record ClearEventLogCommand : IDungeonCommand
    {
        public EntityId ActionTaker => EntityId.Invalid;
        
        public IEnumerable<IDungeonCommand> ApplyCommand(ICommandDungeon world)
        {
            var eventLog = world.WritableComponentStore.AssertGet<IEventLogWriter>();
            eventLog.FlushEventLog();
            return Enumerable.Empty<IDungeonCommand>();
        }
    }
}