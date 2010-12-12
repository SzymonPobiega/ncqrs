using System;
using System.Collections.Generic;
using System.IO;
using Ncqrs.Eventing.Sourcing;
using Ncqrs.Eventing.Storage.NoDB.Tests.Fakes;
using NUnit.Framework;
using Rhino.Mocks;

namespace Ncqrs.Eventing.Storage.NoDB.Tests.EventStoreTests
{
    [Category("Integration")]
    public abstract class NoDBEventStoreTestFixture
    {
        protected NoDBEventStore EventStore;
        protected ISourcedEvent[] Events;
        protected Guid EventSourceId;

        [TestFixtureSetUp]
        public void BaseSetup()
        {
            EventStore = new NoDBEventStore("./");
            EventSourceId = Guid.NewGuid();
            int sequenceCounter = 1;
            Events = new SourcedEvent[]
                         {
                             new CustomerCreatedEvent(Guid.NewGuid(), EventSourceId, sequenceCounter++, DateTime.UtcNow, "Foo",
                                                      35),
                             new CustomerNameChanged(Guid.NewGuid(), EventSourceId, sequenceCounter++, DateTime.UtcNow,
                                                     "Name" + sequenceCounter),
                             new CustomerNameChanged(Guid.NewGuid(), EventSourceId, sequenceCounter++, DateTime.UtcNow,
                                                     "Name" + sequenceCounter)
                         };

            EventStore.Save(Events);
        }
    
        [TestFixtureTearDown]
        public void TearDown()
        {


            Directory.Delete(EventSourceId.ToString().Substring(0, 2), true);
        }
    }
}