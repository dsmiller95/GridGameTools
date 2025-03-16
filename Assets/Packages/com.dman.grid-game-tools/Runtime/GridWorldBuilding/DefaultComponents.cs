using System.Collections.Generic;
using Dman.GridGameTools.EventLog;
using Dman.GridGameTools.PathingData;
using UnityEngine;

namespace Dman.GridGameTools.WorldBuilding
{
    public static class DefaultComponents {
        
        public static readonly ICreateDungeonComponent EventLog = new EventLogCreator();
        
        private class EventLogCreator : ICreateDungeonComponent
        {
            public IEnumerable<IWorldComponent> CreateComponents(WorldComponentCreationContext creationContext)
            {
                yield return new EventLogWorldComponent();
            }
        }
        
        public static readonly ICreateDungeonComponent Pathing = new PathingCreator();
        private class PathingCreator : ICreateDungeonComponent
        {
            public IEnumerable<IWorldComponent> CreateComponents(WorldComponentCreationContext creationContext)
            {
                yield return new DungeonPathingData(creationContext.WorldBounds, Vector3Int.zero);
            }
        }
    }
}