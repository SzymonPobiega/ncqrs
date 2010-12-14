using System;
using System.Collections.Generic;

namespace Ncqrs.Domain.Storage
{
    public class AggregateRootFactory
    {
        private readonly IEnumerable<IAggregateRootCreationStrategy> _strategies;

        public AggregateRootFactory(IEnumerable<IAggregateRootCreationStrategy> strategies)
        {
            _strategies = strategies;
        }

        public AggregateRootFactory(params IAggregateRootCreationStrategy[] strategies)
            : this((IEnumerable<IAggregateRootCreationStrategy>)strategies)
        {
        }

        public AggregateRoot CreateAggregateRoot(Type aggregateRootType)
        {
            foreach (var creationStrategy in _strategies)
            {
                var result = creationStrategy.CreateAggregateRoot(aggregateRootType);
                if (result != null)
                {
                    return result;
                }
            }
            throw new AggregateRootCreationException(string.Format("No strategy can create aggregate root of type {0}",
                                                                   aggregateRootType.AssemblyQualifiedName));
        }
    }
}