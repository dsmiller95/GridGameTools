using System.Collections.Generic;
using Dman.GridGameTools;
using Dman.GridGameTools.EventLog;
using UnityEngine;

namespace Dman.GridGameBindings.WorldComponents
{
    public class CreateEventLogComponent: MonoBehaviour, ICreateDungeonComponent
    {
        public IEnumerable<IWorldComponent> CreateComponents(WorldComponentCreationContext creationContext)
        {
            yield return new EventLogWorldComponent(allowLog: true);
        }
    }
}