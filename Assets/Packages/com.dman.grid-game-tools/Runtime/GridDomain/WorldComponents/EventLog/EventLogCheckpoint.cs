using UnityEngine;

namespace Dman.GridGameTools.EventLog
{
    public struct EventLogCheckpoint
    {
        private int _index;
        
        internal static EventLogCheckpoint Create(int index)
        {
            return new EventLogCheckpoint
            {
                _index = index
            };
        }

        public int EventsUntil(EventLogCheckpoint futureCheckpoint)
        {
            var eventsDelta = futureCheckpoint._index - _index;
            return Mathf.Max(0, eventsDelta);
        }
    }
}