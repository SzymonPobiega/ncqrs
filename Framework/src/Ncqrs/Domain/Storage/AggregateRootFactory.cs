using System;
using System.Collections.Generic;

namespace Ncqrs.Domain.Storage
{
    /// <summary>
    /// A factory responsible for creating aggregate root instances using various configured strategies.
    /// </summary>
    public class AggregateRootFactory : IAggregateRootFactoryTypeSpecification
    {
        private readonly List<CreationStrategyTypeMapping> _strategies = new List<CreationStrategyTypeMapping>();

        /// <summary>
        /// Creates new factory instance.
        /// </summary>
        public AggregateRootFactory()
        {
            RegisterStandardStrategies();
        }       
 
        /// <summary>
        /// Registers built-in standard creation strategies:
        /// <list>
        /// <item><see cref="SimpleAggregateRootCreationStrategy"/></item>
        /// </list>
        /// </summary>
        private void RegisterStandardStrategies()
        {
            SimpleAggregateRootCreationStrategy.Register(this);
        }

        public IAggregateRootFactoryStrategySpecification ForTypes(Func<Type, bool> typeSelector)
        {
            return new AggregateRootFactoryStrategySpecification(this, typeSelector);   
        }        

        /// <summary>
        /// Creates new instance of aggregate root of provided type.
        /// </summary>
        /// <param name="aggregateRootType">Type of aggregate root.</param>
        /// <returns>An instance of <see cref="AggregateRoot"/> derived class that contains logic specified in <paramref name="aggregateRootType"/></returns>
        public AggregateRoot CreateAggregateRoot(Type aggregateRootType)
        {
            foreach (var creationStrategy in _strategies)
            {
                if (creationStrategy.Applies(aggregateRootType))
                {
                    return creationStrategy.Apply(aggregateRootType);
                }                
            }
            throw new AggregateRootCreationException(string.Format("No strategy can create aggregate root of type {0}",
                                                                   aggregateRootType.AssemblyQualifiedName));
        }

        private void RegisterStrategy(Func<Type, bool> typeSelector, IAggregateRootCreationStrategy strategy)
        {
            _strategies.Add(new CreationStrategyTypeMapping(typeSelector, strategy));
        }

        private class CreationStrategyTypeMapping
        {
            private readonly Func<Type, bool> _typeSelector;
            private readonly IAggregateRootCreationStrategy _strategy;

            public CreationStrategyTypeMapping(Func<Type, bool> typeSelector, IAggregateRootCreationStrategy strategy)
            {
                _typeSelector = typeSelector;
                _strategy = strategy;
            }

            public bool Applies(Type type)
            {
                return _typeSelector(type);
            }

            public AggregateRoot Apply(Type type)
            {
                return _strategy.CreateAggregateRoot(type);
            }
        }

        private class AggregateRootFactoryStrategySpecification : IAggregateRootFactoryStrategySpecification
        {
            private readonly AggregateRootFactory _factory;
            private readonly Func<Type, bool> _typeSelector;

            public AggregateRootFactoryStrategySpecification(AggregateRootFactory factory, Func<Type, bool> typeSelector)
            {
                _factory = factory;
                _typeSelector = typeSelector;
            }

            public IAggregateRootFactoryTypeSpecification Use(IAggregateRootCreationStrategy strategy)
            {
                _factory.RegisterStrategy(_typeSelector, strategy);
                return _factory;
            }
        }
    }

    public interface IAggregateRootFactoryTypeSpecification
    {
        IAggregateRootFactoryStrategySpecification ForTypes(Func<Type, bool> typeSelector);
    }

    public interface IAggregateRootFactoryStrategySpecification
    {
        IAggregateRootFactoryTypeSpecification Use(IAggregateRootCreationStrategy strategy);
    }

    
}