using System.Collections.Generic;
using Dman.GridGameTools;
using Dman.GridGameTools.EventLog;

namespace GridDomain.Test
{
    public class EventLogTestHelper : ICreateDungeonComponent
    {
        public static readonly EventLogTestHelper Default = new EventLogTestHelper();
        public IEnumerable<IWorldComponent> CreateComponents(WorldComponentCreationContext creationContext)
        {
            yield return new EventLogWorldComponent();
        }
    }
}