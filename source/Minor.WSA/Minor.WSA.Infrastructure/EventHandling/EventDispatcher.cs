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

        public void DispatchEvent(string jsonMessage)
        {
            var paramObj = JsonConvert.DeserializeObject(jsonMessage, paramType);
            var instance = factory.GetInstance();
            method.Invoke(instance, new object[]{ paramObj });
        }
    }
}