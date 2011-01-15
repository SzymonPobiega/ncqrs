using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using EventStore.Persistence;
using NServiceBus;
using NServiceBus.ObjectBuilder;
using NServiceBus.Unicast.Config;
using NUnit.Framework;

namespace Ncqrs.JOEventStore.NServiceBus.Tests
{
    [TestFixture]
    public class NsbIntegrationTests
    {
        private IBus _bus;

        [Test]
        public void Command_should_be_processed()
        {
            _bus.Send("Ncqrs_JOEventStore_NServiceBus_Tests", new SayHelloCommand
                               {
                                   HelloText = "Hello NCQRS",
                                   CommandId = Guid.NewGuid()
                               });

            SayHelloCommandHandler.SucceededEvent.WaitOne(TimeSpan.FromSeconds(1));
            Assert.IsTrue(SayHelloCommandHandler.Succeeded);
        }

        [SetUp]
        public void Initialize()
        {
            var busUnderConstruction = global::NServiceBus.Configure.With() //for web apps this should be WithWeb()
                .Log4Net()
                .DefaultBuilder()
                .XmlSerializer()
                .MsmqTransport()
                .UnicastBus()
                .LoadMessageHandlers();
            busUnderConstruction.Configurer.ConfigureComponent<InMemoryPersistenceEngine>(ComponentCallModelEnum.Singleton);

            busUnderConstruction.InstallNcqrs();

            _bus = busUnderConstruction
                .CreateBus()
                .Start();
        }
    }
}
