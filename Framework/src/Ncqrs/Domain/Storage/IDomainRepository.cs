using System;
using System.Diagnostics.Contracts;

namespace Ncqrs.Domain.Storage
{
    /// <summary>
    /// A repository that can be used to get and save aggregate roots.
    /// </summary>
    [ContractClass(typeof(IDomainRepositoryContracts))]
    public interface IDomainRepository
    {
        /// <summary>
        /// Gets aggregate root by eventSourceId.
        /// </summary>
        /// <param name="aggregateRootType">Type of the aggregate root.</param>
        /// <param name="eventSourceId">The eventSourceId of the aggregate root.</param>
        /// <returns>A new instance of the aggregate root that contains the latest known state.</returns>
        AggregateRoot GetById(Type aggregateRootType, Guid eventSourceId);

        /// <summary>
        /// Saves the specified aggregate root.
        /// </summary>
        /// <param name="aggregateRootToSave">The aggregate root to save.</param>
        void Save(AggregateRoot aggregateRootToSave);
    }

    [ContractClassFor(typeof(IDomainRepository))]
    internal abstract class IDomainRepositoryContracts : IDomainRepository
    {
        public AggregateRoot GetById(Type aggregateRootType, Guid eventSourceId)
        {
            Contract.Requires<ArgumentNullException>(aggregateRootType != null);

            return default(AggregateRoot);
        }
        
        public void Save(AggregateRoot aggregateRootToSave)
        {
            Contract.Requires<ArgumentNullException>(aggregateRootToSave != null);
        }
    }

}