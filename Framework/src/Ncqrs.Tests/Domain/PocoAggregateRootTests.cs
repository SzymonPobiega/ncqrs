using System;
using System.Linq;
using Ncqrs.Domain;
using Ncqrs.Eventing.Sourcing;
using NUnit.Framework;

namespace Ncqrs.Tests.Domain
{
    [TestFixture]
    public class PocoAggregateRootTests
    {
        public class TestPocoAggregateRoot
        {
            private int _state;
            public virtual Guid Id { get; private set; }

            public virtual void DoSomething(int arguments)
            {
                var value = arguments*2;
                var evnt = new SomethingDone(value);
                OnSomethingDone(evnt);
            }

            protected virtual void OnSomethingDone(SomethingDone evnt)
            {
                _state += evnt.Value;
            }
        }        

        public class SomethingDone : SourcedEvent
        {
            private readonly int _value;

            public SomethingDone(int value)
            {
                _value = value;
            }

            public int Value
            {
                get { return _value; }
            }
        }

        [Test]
        public void Invoking_a_method_should_result_in_publishing_event()
        {
            var root = new PocoAggregateRoot(typeof (TestPocoAggregateRoot));

            ((TestPocoAggregateRoot) root.PublicInterface).DoSomething(5);

            var events = root.GetUncommittedEvents().ToList();
            Assert.AreEqual(1, events.Count);
        }
    }
}