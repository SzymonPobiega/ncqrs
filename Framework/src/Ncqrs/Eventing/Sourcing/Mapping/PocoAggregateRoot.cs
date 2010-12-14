using System;
using Castle.DynamicProxy;
using Ncqrs.Domain;

namespace Ncqrs.Eventing.Sourcing.Mapping
{
    public class PocoAggregateRoot : AggregateRoot
    {
        private static readonly ProxyGenerator _proxyGenerator = new ProxyGenerator();

        public PocoAggregateRoot(Type pocoType) : base()
        {
            
        }

        private object CreatePublicInterface(Type pocoType)
        {
            return _proxyGenerator.CreateClassProxy(pocoType, )
        }

        public PocoAggregateRoot(Type pocoType, Guid id) : base()
        {
            
        }
    }
}