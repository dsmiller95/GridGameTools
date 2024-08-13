using System.Collections.Generic;
using Dman.GridGameTools.EventLog;
using UnityEngine;
using WorldCreation;

namespace Dman.GridGameBindings.WorldComponents
{
    public class CreateEventLogComponent: MonoBehaviour, ICreateDungeonComponent
    {
        public IEnumerable<IWorldComponent> CreateComponents()
        {
            yield return new EventLogWorldComponent(allowLog: true);
        }
    }
}