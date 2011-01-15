using System;
using System.Reflection;

namespace Ncqrs.Domain.Storage
{
    public class SimpleAggregateRootCreationStrategy : IAggregateRootCreationStrategy
    {
        private const BindingFlags ConstructorBindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        public AggregateRoot CreateAggregateRoot(Type aggregateRootType)
        {
            CheckIsSubclassOfAggregateRoot(aggregateRootType);

            var ctor = aggregateRootType.GetConstructor(ConstructorBindingFlags, null, Type.EmptyTypes, null);

            // If there was no ctor found, throw exception.
            if (ctor == null)
            {
                var message = String.Format("No constructor found on aggregate root type {0} that accepts " +
                                            "no parameters.", aggregateRootType.AssemblyQualifiedName);
                throw new AggregateRootCreationException(message);
            }

            // There was a ctor found, so invoke it and return the instance.
            var aggregateRoot = (AggregateRoot)ctor.Invoke(null);

            return aggregateRoot;
        }

        private static void CheckIsSubclassOfAggregateRoot(Type aggregateRootType)
        {
            if (!aggregateRootType.IsSubclassOf(typeof(AggregateRoot)))
            {
                var msg = string.Format("Specified type {0} is not a subclass of AggregateRoot class.", aggregateRootType.FullName);
                throw new ArgumentOutOfRangeException("aggregateRootType", msg);
            }
        }

        public AggregateRoot CreateAggregateRoot(Type aggregateRootType, Guid id)
        {
            CheckIsSubclassOfAggregateRoot(aggregateRootType);

            var ctor = aggregateRootType.GetConstructor(ConstructorBindingFlags, null, new[] { typeof(Guid) }, null);

            // If there was no ctor found, throw exception.
            if (ctor == null)
            {
                var message = String.Format("No constructor found on aggregate root type {0} that accepts " +
                                            "a single Guid (id) parameter.", aggregateRootType.AssemblyQualifiedName);
                throw new AggregateRootCreationException(message);
            }

            // There was a ctor found, so invoke it and return the instance.
            var aggregateRoot = (AggregateRoot)ctor.Invoke(new object[]{id});

            return aggregateRoot;
        }

        public T CreateAggregateRoot<T>() where T : AggregateRoot
        {
            return (T) CreateAggregateRoot(typeof (T));
        }
    }
}
