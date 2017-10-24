using System.Reflection;

namespace Minor.WSA.Infrastructure
{
    internal class GenericEventDispatcher : IEventDispatcher
    {
        private IFactory factory;
        private MethodInfo method;

        public GenericEventDispatcher(IFactory factory, MethodInfo method)
        {
            this.factory = factory;
            this.method = method;
        }

        public void DispatchEvent(EventMessage eventMessage)
        {
            var instance = factory.GetInstance();
            method.Invoke(instance, new object[] { eventMessage });
        }
    }
}