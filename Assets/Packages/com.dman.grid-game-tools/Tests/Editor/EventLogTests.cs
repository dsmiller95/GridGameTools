using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Dman.GridGameTools.DataStructures;
using Dman.GridGameTools.Entities;
using Dman.GridGameTools.EventLog;
using NUnit.Framework;
using UnityEngine;

namespace Dman.GridGameTools.Tests
{
    public record TestEvent : IGridEvent
    {
        public EntityId Entity { get; set; }
        public Vector3Int Point { get; set; }
        public string Description { get; set; }

        public TestEvent()
        {
            Entity = EntityId.New();
            Point = new Vector3Int(0, 0, 0);
            Description = "";
        }

        public override string ToString()
        {
            return $"[TestEvent: {Entity} at {Point} - {Description}]";
        }
    }
    
    public class EventLogTests
    {
        private EventLogWorldComponent eventLog;
        private IEventLogWriter writer;

        private void InitWriter()
        {
            eventLog = new EventLogWorldComponent();
            writer = eventLog.GetWriter() as IEventLogWriter;
            Assert.IsNotNull(writer);
        }

        private void BakeAndSwap()
        {
            var baked = (writer as IWorldComponentWriter)?.BakeImmutable(andDispose: false) as EventLogWorldComponent;
            Assert.IsNotNull(baked);
            eventLog = baked;
            writer = eventLog.GetWriter() as IEventLogWriter;
        }

        private void AssertEventLogIs(params TestEvent[] expected)
        {
            AssertTestEventSequenceEquals(expected, writer.AllEvents);
            var baked = (writer as IWorldComponentWriter)?.BakeImmutable(andDispose: false) as IEventLog;
            Assert.IsNotNull(baked);
            AssertTestEventSequenceEquals(expected, baked.AllEvents);
        }
        private void AssertEventLogIs(EventLogCheckpoint sinceCheckpoint, params TestEvent[] expected)
        {
            AssertTestEventSequenceEquals(expected, writer.AllEventsSince(sinceCheckpoint));
            var baked = (writer as IWorldComponentWriter)?.BakeImmutable(andDispose: false) as IEventLog;
            Assert.IsNotNull(baked);
            AssertTestEventSequenceEquals(expected, baked.AllEventsSince(sinceCheckpoint));
        }

        private void AssertTestEventSequenceEquals(IEnumerable<IGridEvent> expectedEvents, IEnumerable<IGridEvent> actual)
        {
            var expectedStr = string.Join("\n", expectedEvents);
            var actualStr = string.Join("\n", actual);
            Assert.AreEqual(expectedStr, actualStr, $"expected \n{expectedStr}\n but got \n{actualStr}");
        }
        
        private void AssertCompleteEventCount(int expected)
        {
            Assert.AreEqual(expected, writer.CompleteEventCount);
            var baked = (writer as IWorldComponentWriter)?.BakeImmutable(andDispose: false) as IEventLog;
            Assert.IsNotNull(baked);
            Assert.AreEqual(expected, baked.CompleteEventCount);
        }
        
        [Test]
        public void WhenEventLogged_CanRetrieve()
        {
            // arrange
            InitWriter();
            var event1 = new TestEvent();
            var event2 = new TestEvent();
            var event3 = new TestEvent();
            
            // act
            writer.LogEvent(event1);
            writer.LogEvent(event2);
            writer.LogEvent(event3);
            
            // assert
            AssertCompleteEventCount(3);
            AssertEventLogIs(event1, event2, event3);
        }
        
        
        [Test]
        public void WhenEventLogged_WithBaking_CanRetrieve()
        {
            // arrange
            InitWriter();
            var event1 = new TestEvent();
            var event2 = new TestEvent();
            var event3 = new TestEvent();
            
            // act
            writer.LogEvent(event1);
            BakeAndSwap();
            writer.LogEvent(event2);
            BakeAndSwap();
            writer.LogEvent(event3);
            
            // assert
            AssertCompleteEventCount(3);
            AssertEventLogIs(event1, event2, event3);
        }
        
        
        [Test]
        public void WhenEventLogged_ThenCleared_CannotRetrieve()
        {
            // arrange
            InitWriter();
            var event1 = new TestEvent();
            var event2 = new TestEvent();
            var event3 = new TestEvent();
            var event4 = new TestEvent();
            
            // act
            writer.LogEvent(event1);
            writer.LogEvent(event2);
            writer.FlushEventLog();
            writer.LogEvent(event3);
            writer.LogEvent(event4);
            
            // assert
            AssertCompleteEventCount(4);
            AssertEventLogIs(event3, event4);
        }
        
        
        
        
        [Test]
        public void WhenEventCheckPointed_WhileWriting_KeepsCheckpoint()
        {
            // arrange
            InitWriter();
            var event1 = new TestEvent();
            var event2 = new TestEvent();
            var event3 = new TestEvent();
            
            // act
            var firstCheckpoint = writer.Checkpoint();
            writer.LogEvent(event1);
            writer.LogEvent(event2);
            var secondCheckpoint = writer.Checkpoint();
            writer.LogEvent(event3);
            
            // assert
            AssertCompleteEventCount(3);
            AssertEventLogIs(firstCheckpoint, event1, event2, event3);
            AssertEventLogIs(secondCheckpoint, event3);
        }
        
        
        [Test]
        public void WhenEventCheckPointed_WhileWriting_AndBaking_KeepsCheckpoint()
        {
            // arrange
            InitWriter();
            var event1 = new TestEvent();
            var event2 = new TestEvent();
            var event3 = new TestEvent();
            
            // act
            var firstCheckpoint = writer.Checkpoint();
            writer.LogEvent(event1);
            BakeAndSwap();
            writer.LogEvent(event2);
            BakeAndSwap();
            var secondCheckpoint = writer.Checkpoint();
            BakeAndSwap();
            writer.LogEvent(event3);
            BakeAndSwap();
            
            // assert
            AssertCompleteEventCount(3);
            AssertEventLogIs(firstCheckpoint, event1, event2, event3);
            AssertEventLogIs(secondCheckpoint, event3);
        }
        
        
        [Test]
        public void WhenEventCheckPointed_ThenCleared_KeepsCheckpoint()
        {
            // arrange
            InitWriter();
            var event1 = new TestEvent();
            var event2 = new TestEvent();
            var event3 = new TestEvent();
            
            // act
            var firstCheckpoint = writer.Checkpoint();
            writer.LogEvent(event1);
            writer.FlushEventLog();
            writer.LogEvent(event2);
            var secondCheckpoint = writer.Checkpoint();
            writer.LogEvent(event3);
            
            // assert
            AssertCompleteEventCount(3);
            AssertEventLogIs(firstCheckpoint, event2, event3);
            AssertEventLogIs(secondCheckpoint, event3);
        }
        
        [Test]
        public void WhenEventCheckPointed_ThenCleared_AndBaked_KeepsCheckpoint()
        {
            // arrange
            InitWriter();
            var event1 = new TestEvent();
            var event2 = new TestEvent();
            var event3 = new TestEvent();
            
            // act
            var firstCheckpoint = writer.Checkpoint();
            writer.LogEvent(event1);
            writer.FlushEventLog();
            writer.LogEvent(event2);
            BakeAndSwap();
            var secondCheckpoint = writer.Checkpoint();
            writer.LogEvent(event3);
            
            // assert
            AssertCompleteEventCount(3);
            AssertEventLogIs(firstCheckpoint, event2, event3);
            AssertEventLogIs(secondCheckpoint, event3);
        }
    }
}