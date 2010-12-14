using System;
using System.Linq;

namespace Ncqrs.Domain.Storage
{
    public interface IAggregateRootCreationStrategy
    {
        AggregateRoot CreateAggregateRoot(Type aggregateRootType);
    }
}
