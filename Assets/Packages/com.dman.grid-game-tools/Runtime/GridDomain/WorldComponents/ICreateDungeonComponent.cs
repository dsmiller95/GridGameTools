using System.Collections.Generic;

namespace Dman.GridGameTools
{
    public interface ICreateDungeonComponent
    {
        public IEnumerable<IWorldComponent> CreateComponents(WorldComponentCreationContext creationContext);
    }
}