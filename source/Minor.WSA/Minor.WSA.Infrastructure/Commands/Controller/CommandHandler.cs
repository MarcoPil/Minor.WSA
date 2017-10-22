using System;
using System.Reflection;

namespace Minor.WSA.Infrastructure
{
    public class CommandHandler : ICommandHandler
    {
        private IFactory factory;
        private MethodInfo method;
        private Type paramType;

        public CommandHandler(IFactory factory, MethodInfo method, Type paramType)
        {
            this.factory = factory;
            this.method = method;
            this.paramType = paramType;
        }
    }
}