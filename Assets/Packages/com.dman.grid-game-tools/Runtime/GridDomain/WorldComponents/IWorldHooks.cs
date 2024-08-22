using Dman.GridGameTools.Entities;

namespace Dman.GridGameTools
{
    public interface IWorldHooks
    {
        public void EntityChange(EntityWriteRecord writeRecord, IEntityStore upToDateStore);
    }
}