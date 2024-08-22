using System.Collections.Generic;
using Dman.GridGameTools;

namespace WorldCreation
{
    public interface ICreateDungeonComponent
    {
        public IEnumerable<IWorldComponent> CreateComponents(WorldComponentCreationContext creationContext);
    }
}