using System.Collections.Generic;
using System.Reflection;
using Castle.Core.Interceptor;
using Ncqrs.Domain;

namespace Ncqrs.Eventing.Sourcing.Mapping
{
    public class ApplyEventInterceptor : IInterceptor
    {
        private readonly AggregateRoot _aggregateRoot;
        private readonly List<MethodBase> _eventHandlers = new List<MethodBase>();

        public ApplyEventInterceptor(AggregateRoot aggregateRoot)
        {
            _aggregateRoot = aggregateRoot;
        }

        public void Intercept(IInvocation invocation)
        {
            if (!_eventHandlers.Contains(invocation.Method))
            {
                invocation.Proceed();
            }
            else
            {
                _aggregateRoot.ApplyEvent((ISourcedEvent)invocation.Arguments[0]);
            }
        }

        public void RegisterHandler(MethodBase handlingMethodMetadata)
        {
            _eventHandlers.Add(handlingMethodMetadata);
        }
    }
}