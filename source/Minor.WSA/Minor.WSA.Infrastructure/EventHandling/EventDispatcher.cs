using System;
using System.Reflection;

namespace Minor.WSA.Infrastructure
{
    public class EventDispatcher
    {
        private Factory factory;
        private MethodInfo method;
        private Type paramType;

        public EventDispatcher(Factory factory, MethodInfo method, Type paramType)
        {
            this.factory = factory;
            this.method = method;
            this.paramType = paramType;
        }

        public void DispatchEvent(string jsonMessage)
        {
            throw new NotImplementedException();
        }
    }
}