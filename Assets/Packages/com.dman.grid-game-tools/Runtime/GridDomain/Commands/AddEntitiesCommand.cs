using System;
using System.Collections.Generic;
using Dman.GridGameTools.Entities;
using JetBrains.Annotations;

namespace Dman.GridGameTools.Commands
{
    public class AddEntitiesCommand : IDungeonCommand
    {
        private readonly IDungeonEntity[] _entities;
        public EntityId ActionTaker => null;
        public MovementExpectation ExpectsToCauseMovement => MovementExpectation.WillNotMove;
        [CanBeNull] public EntityId[] AddedEntities { get; private set; }
        public AddEntitiesCommand(params IDungeonEntity[] entities)
        {
            _entities = entities;
            AddedEntities = null;
        }
    
        public IEnumerable<IDungeonCommand> ApplyCommand(ICommandDungeon world)
        {
            var addedEntities = new List<EntityId>();
            foreach (IDungeonEntity addedEntity in _entities)
            {
                var id = world.CreateEntity(addedEntity);
                addedEntities.Add(id);
            }

            this.AddedEntities = addedEntities.ToArray();
        
            return Array.Empty<IDungeonCommand>();
        }
    }
}