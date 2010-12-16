using System;
using Ncqrs.Eventing.Sourcing.Mapping;

namespace Ncqrs.Domain.Storage
{
    public class PocoAggregateRootCreationStrategy : IAggregateRootCreationStrategy
    {
        public AggregateRoot CreateAggregateRoot(Type aggregateRootType)
        {
            return new PocoAggregateRoot(aggregateRootType);
        }
    }
}