using System.Collections.Generic;
using Dman.GridGameTools;

namespace Dman.GridGameBindings.WorldComponents
{
    public interface ICreateDungeonComponent
    {
        public IEnumerable<IWorldComponent> CreateComponents(WorldComponentCreationContext creationContext);
    }
}