using System;
using Ncqrs.Domain;
using NServiceBus;

namespace Ncqrs.JOEventStore.NServiceBus
{
    public abstract class CommandHandlerBase<TMessage, TAggregateRoot> : IHandleMessages<TMessage>
        where TMessage : IMessage
        where TAggregateRoot : AggregateRoot
    {
        public IRemoteFacade RemoteFacade { get; set; }

        public void Handle(TMessage message)
        {
            var metadata = ExtractMetadata(message);
            if (metadata.TargetType == null)
            {
                metadata.TargetType = typeof (TAggregateRoot);
            }
            RemoteFacade.Execute(metadata, x => Handle(message, (TAggregateRoot)x));
        }

        protected abstract CommandMetadata ExtractMetadata(TMessage message);
        protected abstract void Handle(TMessage message, TAggregateRoot aggregateRoot);
    }
}