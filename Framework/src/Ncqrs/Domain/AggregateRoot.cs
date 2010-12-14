using System;
using Ncqrs.Eventing.Sourcing;
using Ncqrs.Eventing.Sourcing.Mapping;

namespace Ncqrs.Domain
{
    /// <summary>
    /// The abstract concept of an aggregate root.
    /// </summary>
    public abstract class AggregateRoot : EventSource
    {
        private object _publicInterface;

        /// <summary>
        /// Occurs when an event was applied to an <see cref="AggregateRoot"/>.
        /// </summary>
        internal static event EventHandler<EventAppliedArgs> EventApplied;

        protected AggregateRoot()
        {
            _publicInterface = this;
        }        

        protected AggregateRoot(Guid id) : base(id)
        {
            _publicInterface = this;
        }        

        public object PublicInterface
        {
            get { return _publicInterface; }
            protected set { _publicInterface = value; }
        }

        [NoEventHandler]
        protected override void OnEventApplied(ISourcedEvent appliedEvent)
        {
            if(EventApplied != null)
            {
                EventApplied(this, new EventAppliedArgs(appliedEvent));
            }
        }
    }
}