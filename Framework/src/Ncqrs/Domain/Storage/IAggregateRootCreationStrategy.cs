using System;

namespace Ncqrs.Domain.Storage
{
    public interface IAggregateRootCreationStrategy
    {
        AggregateRoot CreateAggregateRoot(Type aggregateRootType);
        AggregateRoot CreateAggregateRoot(Type aggregateRootType, Guid id);
        T CreateAggregateRoot<T>() where T : AggregateRoot;
    }
}
