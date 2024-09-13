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
        /// <summary>
        /// the count of all events every created, including flushed events
        /// </summary>
        public int CompleteEventCount { get; }

        public EventLogCheckpoint Checkpoint();
        public IEnumerable<IGridEvent> AllEventsSince(EventLogCheckpoint checkpoint);
    }
    
    public class EventLogWorldComponent : IEventLog, IWorldComponent
    {
        private List<IGridEvent> _events = new();
        private bool _isDisposed = false;
        public IEnumerable<IGridEvent> AllEvents => _events;
        public bool AllowLog { get; }
        public int CompleteEventCount { get; }
        public EventLogCheckpoint Checkpoint() => EventLogCheckpoint.Create(CompleteEventCount);

        public IEnumerable<IGridEvent> AllEventsSince(EventLogCheckpoint checkpoint)
        {
            return AllEvents;
        }

        public EventLogWorldComponent(bool allowLog = true)
        {
            AllowLog = allowLog;
            CompleteEventCount = 0;
        }
        private EventLogWorldComponent(IEnumerable<IGridEvent> existingEvents, int completeEventCount, bool allowLog)
        {
            _events = existingEvents.ToList();
            AllowLog = allowLog;
            CompleteEventCount = completeEventCount;
        }
        
        public IWorldComponentWriter GetWriter()
        {
            if (_isDisposed) throw new ObjectDisposedException("EventLogWorldComponent");
            return new EventLogWriterWorldComponent(this);
        }

        public void Dispose()
        {
            _isDisposed = true;
        }
        
        private class EventLogWriterWorldComponent : IEventLogWriter, IWorldComponentWriter
        {
            private readonly EventLogWorldComponent _baseEventLog;
            private bool _didFlushHistory = false;
            private List<IGridEvent> _addedEvents;
            private bool _isDisposed;
            public bool AllowLog { get; private set; }
            public int CompleteEventCount { get; private set; }
            public EventLogCheckpoint Checkpoint() => EventLogCheckpoint.Create(CompleteEventCount);

            public IEnumerable<IGridEvent> AllEventsSince(EventLogCheckpoint checkpoint)
            {
                if (_isDisposed) throw new ObjectDisposedException("EventLogWriterWorldComponent");
                return AllEvents;
            }

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
                CompleteEventCount = baseEventLog.CompleteEventCount;
            }

            public void LogEvent(IGridEvent gridEvent)
            {
                if (_isDisposed) throw new ObjectDisposedException("EventLogWriterWorldComponent");
                if (!AllowLog || gridEvent == null) return;
                CompleteEventCount++;
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
                return new EventLogWorldComponent(AllEvents, CompleteEventCount, AllowLog);
            }

            public void Dispose()
            {
                _isDisposed = true;
                _addedEvents = null;
            }
        }
    }
}