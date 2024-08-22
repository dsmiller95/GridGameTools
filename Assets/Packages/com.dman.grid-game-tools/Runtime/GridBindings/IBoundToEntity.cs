using System;
using Dman.GridGameTools;
using Dman.GridGameTools.Entities;
using UnityEngine;

namespace Dman.GridGameBindings
{
    public interface IBoundToEntity
    {
        public Type BoundType { get; }
        public EntityId EntityId { get; }
        public IDungeonEntity GetEntityObjectGuessGeneric(IDungeonToWorldContext context, Transform componentGenerators);
        public bool TryBind(EntityId entity, IDungeonUpdater updater, IDungeonToWorldContext worldContext, IDungeonWorld world);
    }
}