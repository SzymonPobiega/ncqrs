using System;
using Ncqrs.Eventing.Sourcing.Mapping;

namespace Ncqrs.Domain.Storage
{
    /// <summary>
    /// A strategy for creating POCO aggregate roots.
    /// </summary>
    public class PocoAggregateRootCreationStrategy : IAggregateRootCreationStrategy
    {
        private readonly IEventHandlerMappingStrategy _eventHandlerMappingStrategy;

        /// <summary>
        /// Creates new strategy instance for specified event handler mapping strategy.
        /// </summary>
        /// <param name="eventHandlerMappingStrategy">A event handler discovery and mapping strategy to be used against the POCO class.</param>
        public PocoAggregateRootCreationStrategy(IEventHandlerMappingStrategy eventHandlerMappingStrategy)
        {
            _eventHandlerMappingStrategy = eventHandlerMappingStrategy;
        }

        public AggregateRoot CreateAggregateRoot(Type aggregateRootType)
        {
            return new PocoAggregateRoot(aggregateRootType, _eventHandlerMappingStrategy);
        }
    }
}