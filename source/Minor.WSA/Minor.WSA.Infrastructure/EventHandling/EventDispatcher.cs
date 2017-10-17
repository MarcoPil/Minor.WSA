using Newtonsoft.Json;
using System;
using System.Reflection;

namespace Minor.WSA.Infrastructure
{
    public class EventDispatcher
    {
        private IFactory factory;
        private MethodInfo method;
        private Type paramType;

        public EventDispatcher(IFactory factory, MethodInfo method, Type paramType)
        {
            this.factory = factory;
            this.method = method;
            this.paramType = paramType;
        }

        public virtual void DispatchEvent(EventMessage eventMessage)
        {
            var paramObj = JsonConvert.DeserializeObject(eventMessage.JsonMessage, paramType);
            var instance = factory.GetInstance();
            method.Invoke(instance, new object[]{ paramObj });
        }
    }
}