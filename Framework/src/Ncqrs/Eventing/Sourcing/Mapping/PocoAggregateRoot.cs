using System;
using Castle.DynamicProxy;
using Ncqrs.Domain;

namespace Ncqrs.Eventing.Sourcing.Mapping
{
    public class PocoAggregateRoot : AggregateRoot
    {
        [NonSerialized]
        private static readonly ProxyGenerator _proxyGenerator = new ProxyGenerator();        
        [NonSerialized] 
        private static readonly IEventHandlerMappingStrategy _mappingStrategy = new ConventionBasedEventHandlerMappingStrategy();

        public PocoAggregateRoot(Type pocoType)
            : base()
        {
            var eventInterceptor = new ApplyEventInterceptor(this);
            var idInterceptor = new IdInterceptor(this);
            var poco = _proxyGenerator.CreateClassProxy(pocoType, eventInterceptor, idInterceptor);

            foreach (var handler in _mappingStrategy.GetEventHandlers(poco))
            {
                RegisterHandler(handler);
                eventInterceptor.RegisterHandler(handler.GetHandlingMethod());
            }

            PublicInterface = poco;
        }
                
    }
}