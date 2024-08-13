using UnityEngine;

namespace Dman.GridGameTools.EventLog
{
    public interface IGridEvent
    {
        public EntityId Entity { get; }
        public Vector3Int Point { get; }
    }
}
