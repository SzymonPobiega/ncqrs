using Castle.Core.Interceptor;

namespace Ncqrs.Domain
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