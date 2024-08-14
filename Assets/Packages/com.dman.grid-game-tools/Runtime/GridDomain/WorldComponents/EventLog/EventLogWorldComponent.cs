using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Dman.GridGameTools.EventLog
{
    public interface IEventLogWriter : IEventLog
    {
        public void LogEvent([CanBeNull] IGridEvent gridEvent);
        public void FlushEventLog();
        public void SetAllowLog(bool allowWrites);

    }
    public interface IEventLog
    {
        public IEnumerable<IGridEvent> AllEvents { get; }
        public bool AllowLog { get; }
    }
    
    public class EventLogWorldComponent : IEventLog, IWorldComponent
    {
        public IEnumerable<IGridEvent> AllEvents => _events;
        public bool AllowLog { get; }
        private List<IGridEvent> _events = new();

        public EventLogWorldComponent(bool allowLog = true)
        {
            AllowLog = allowLog;
        }
        private EventLogWorldComponent(IEnumerable<IGridEvent> existingEvents, bool allowLog)
        {
            _events = existingEvents.ToList();
            AllowLog = allowLog;
        }
        
        public IWorldComponentWriter GetWriter()
        {
            return new EventLogWriterWorldComponent(this);
        }
        public void LogEvent(IGridEvent gridEvent)
        {
            _events.Add(gridEvent);
        }
        
        public class EventLogWriterWorldComponent : IEventLogWriter, IWorldComponentWriter
        {
            private readonly EventLogWorldComponent _baseEventLog;
            private bool didFlushHistory = false;
            public IEnumerable<IGridEvent> AllEvents
            {
                get
                {
                    if (didFlushHistory) return _addedEvents;
                    
                    return _baseEventLog.AllEvents.Concat(_addedEvents);
                }
            }

            public bool AllowLog { get; private set; }

            private List<IGridEvent> _addedEvents;

            public EventLogWriterWorldComponent(EventLogWorldComponent baseEventLog)
            {
                _baseEventLog = baseEventLog;
                _addedEvents = new List<IGridEvent>();
                AllowLog = baseEventLog.AllowLog;
            }

            public void LogEvent(IGridEvent gridEvent)
            {
                if (!AllowLog || gridEvent == null) return;
                _addedEvents.Add(gridEvent);
            }

            public void FlushEventLog()
            {
                didFlushHistory = true;
                _addedEvents.Clear();
            }

            public void SetAllowLog(bool allowWrites)
            {
                AllowLog = allowWrites;
            }

            public IWorldComponent BakeImmutable()
            {
                return new EventLogWorldComponent(AllEvents, AllowLog);
            }
        }
    }
}