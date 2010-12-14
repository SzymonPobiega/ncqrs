using Castle.Core.Interceptor;
using Ncqrs.Domain;

namespace Ncqrs.Eventing.Sourcing.Mapping
{
    public class IdInterceptor : IInterceptor
    {
        private readonly AggregateRoot _aggregateRoot;

        public IdInterceptor(AggregateRoot aggregateRoot)
        {
            _aggregateRoot = aggregateRoot;
        }

        public void Intercept(IInvocation invocation)
        {
            if (invocation.Method.IsSpecialName && invocation.Method.Name == "get_Id")
            {
                //Check type == Guid
                invocation.ReturnValue = _aggregateRoot.EventSourceId;
            }
            else
            {
                invocation.Proceed();
            }
        }
    }
}