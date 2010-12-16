using System;
using System.Diagnostics;
using System.Reflection;

namespace Ncqrs.Domain.Storage
{
    public class SimpleAggregateRootCreationStrategy : IAggregateRootCreationStrategy
    {
        public static void Register(IAggregateRootFactoryTypeSpecification factory)
        {
            factory.ForTypes(IsSubclassOfAggregateRoot).Use(new SimpleAggregateRootCreationStrategy());
        }

        public AggregateRoot CreateAggregateRoot(Type aggregateRootType)
        {
            if (!IsSubclassOfAggregateRoot(aggregateRootType))
            {
                var msg = string.Format("Specified type {0} is not a subclass of AggregateRoot class.", aggregateRootType.FullName);
                throw new ArgumentException(msg, "aggregateRootType");
            }

            // Flags to search for a public and non public contructor.
            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            // Get the constructor that we want to invoke.
            var ctor = aggregateRootType.GetConstructor(flags, null, Type.EmptyTypes, null);

            // If there was no ctor found, throw exception.
            if (ctor == null)
            {
                var msg = String.Format("No constructor found on aggregate root type {0} that accepts " +
                                            "no parameters.", aggregateRootType.AssemblyQualifiedName);
                throw new AggregateRootCreationException(msg);
            }

            // There was a ctor found, so invoke it and return the instance.
            var aggregateRoot = (AggregateRoot)ctor.Invoke(null);

            return aggregateRoot;
        }

        private static bool IsSubclassOfAggregateRoot(Type aggregateRootType)
        {
            return aggregateRootType.IsSubclassOf(typeof(AggregateRoot));
        }
    }
}
