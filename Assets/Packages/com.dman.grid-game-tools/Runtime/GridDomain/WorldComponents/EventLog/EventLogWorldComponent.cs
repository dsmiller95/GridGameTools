using System;
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
        
        private class EventLogWriterWorldComponent : IEventLogWriter, IWorldComponentWriter
        {
            private readonly EventLogWorldComponent _baseEventLog;
            private bool _didFlushHistory = false;
            private List<IGridEvent> _addedEvents;
            private bool _isDisposed;
            public bool AllowLog { get; private set; }
            
            public IEnumerable<IGridEvent> AllEvents
            {
                get
                {
                    if (_didFlushHistory) return _addedEvents;
                    
                    return _baseEventLog.AllEvents.Concat(_addedEvents);
                }
            }

            public EventLogWriterWorldComponent(EventLogWorldComponent baseEventLog)
            {
                _baseEventLog = baseEventLog;
                _addedEvents = new List<IGridEvent>();
                AllowLog = baseEventLog.AllowLog;
            }

            public void LogEvent(IGridEvent gridEvent)
            {
                if (_isDisposed) throw new ObjectDisposedException("EventLogWriterWorldComponent");
                if (!AllowLog || gridEvent == null) return;
                _addedEvents.Add(gridEvent);
            }

            public void FlushEventLog()
            {
                if (_isDisposed) throw new ObjectDisposedException("EventLogWriterWorldComponent");
                _didFlushHistory = true;
                _addedEvents.Clear();
            }

            public void SetAllowLog(bool allowWrites)
            {
                if (_isDisposed) throw new ObjectDisposedException("EventLogWriterWorldComponent");
                AllowLog = allowWrites;
            }

            public IWorldComponent BakeImmutable(bool andDispose)
            {
                if (_isDisposed) throw new ObjectDisposedException("EventLogWriterWorldComponent");
                if (andDispose) _isDisposed = true;
                return new EventLogWorldComponent(AllEvents, AllowLog);
            }

            public void Dispose()
            {
                _isDisposed = true;
                _addedEvents = null;
            }
        }
    }
}