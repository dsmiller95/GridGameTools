using Dman.GridGameTools.DataStructures;
using Dman.GridGameTools.Entities;

namespace Dman.GridGameTools
{
    public static class Pools
    {
        internal static readonly ListPool<EntityId> EntityIdLists = ListPool<EntityId>.Create(initialCapacity: 10);
    }
}