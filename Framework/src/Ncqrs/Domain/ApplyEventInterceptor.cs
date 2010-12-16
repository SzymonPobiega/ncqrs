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
            if (_isApplying || !IsEventHandler(invocation))
            {
                invocation.Proceed();
            }
            else
            {
                RedirectToApply(invocation);
            }
        }

        private void RedirectToApply(IInvocation invocation)
        {
            _isApplying = true;
            try
            {
                _aggregateRoot.ApplyEvent((ISourcedEvent)invocation.Arguments[0]);
            }
            finally 
            {
                _isApplying = false;   
            }
        }

        private bool IsEventHandler(IInvocation invocation)
        {
            return _eventHandlers.Any(x => x.Name == invocation.Method.Name);
        }

        public void RegisterHandler(MethodBase handlingMethodMetadata)
        {
            _eventHandlers.Add(handlingMethodMetadata);
        }
    }
}