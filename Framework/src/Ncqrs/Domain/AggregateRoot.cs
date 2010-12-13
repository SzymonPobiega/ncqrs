using System;
using Ncqrs.Eventing.Sourcing;
using Ncqrs.Eventing.Sourcing.Mapping;

namespace Ncqrs.Domain
{
    public class Customer
    {


        public Customer()
        {
            PocoAggRoot.Register(this);   
        }

        protected void OnMyEvent(MyEvent e)
        {
            // handle event, update state.
        }
    }

    public class PocoAggRoot : AggregateRoot
    {
        
    }

    /// <summary>
    /// The abstract concept of an aggregate root.
    /// </summary>
    public abstract class AggregateRoot : EventSource
    {
        protected AggregateRoot()
        { }

        protected AggregateRoot(Guid id)
            : base(id)
        { }
    }
}