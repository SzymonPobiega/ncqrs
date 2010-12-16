using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Castle.Core.Interceptor;
using Ncqrs.Eventing.Sourcing;

namespace Ncqrs.Domain
{
    public class ApplyEventInterceptor : IInterceptor
    {
        private bool _isApplying;
        private readonly AggregateRoot _aggregateRoot;
        private readonly List<MethodBase> _eventHandlers = new List<MethodBase>();

        public ApplyEventInterceptor(AggregateRoot aggregateRoot)
        {
            _aggregateRoot = aggregateRoot;
        }

        public void Intercept(IInvocation invocation)
        {
            if (_isApplying || !_eventHandlers.Any(x => x.Name == invocation.Method.Name))
            {
                invocation.Proceed();
            }
            else
            {
                _isApplying = true;
                _aggregateRoot.ApplyEvent((ISourcedEvent)invocation.Arguments[0]);
                _isApplying = false;   
            }
        }

        public void RegisterHandler(MethodBase handlingMethodMetadata)
        {
            _eventHandlers.Add(handlingMethodMetadata);
        }
    }
}