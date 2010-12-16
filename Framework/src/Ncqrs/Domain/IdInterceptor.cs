using System;
using System.Reflection;
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
            if (IsIdPropertyGetter(invocation.Method))
            {
                VerifySignature(invocation.Method);
                invocation.ReturnValue = _aggregateRoot.EventSourceId;
            }
            else
            {
                invocation.Proceed();
            }
        }

        private static void VerifySignature(MethodInfo invocationTarget)
        {
            if (invocationTarget.ReturnType != typeof(Guid))
            {
                throw new InvalidOperationException("Id property must be of System.Guid type.");
            }
        }

        private static bool IsIdPropertyGetter(MethodBase invocationTarget)
        {
            return invocationTarget.IsSpecialName && invocationTarget.Name == "get_Id";
        }
    }
}