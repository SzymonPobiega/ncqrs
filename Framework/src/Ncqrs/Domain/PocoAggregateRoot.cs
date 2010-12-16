using System;
using Castle.DynamicProxy;
using Ncqrs.Eventing.Sourcing.Mapping;

namespace Ncqrs.Domain
{
    public class PocoAggregateRoot : AggregateRoot
    {
        [NonSerialized]
        private static readonly ProxyGenerator _proxyGenerator = new ProxyGenerator();        

        public PocoAggregateRoot(Type pocoType, IEventHandlerMappingStrategy eventHandlerMappingStrategy)
        {
            var eventInterceptor = new ApplyEventInterceptor(this);
            var idInterceptor = new IdInterceptor(this);
            var poco = _proxyGenerator.CreateClassProxy(pocoType, eventInterceptor, idInterceptor);

            foreach (var handler in eventHandlerMappingStrategy.GetEventHandlers(poco))
            {
                RegisterHandler(handler);
                eventInterceptor.RegisterHandler(handler.GetHandlingMethod());
            }

            PublicInterface = poco;
        }
                
    }
}