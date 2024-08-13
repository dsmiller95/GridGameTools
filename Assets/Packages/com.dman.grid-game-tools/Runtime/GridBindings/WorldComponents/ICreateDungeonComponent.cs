using System.Collections.Generic;

namespace WorldCreation
{
    public interface ICreateDungeonComponent
    {
        public IEnumerable<IWorldComponent> CreateComponents();
    }
}