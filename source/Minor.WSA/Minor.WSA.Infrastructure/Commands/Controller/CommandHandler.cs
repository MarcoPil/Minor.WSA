using Newtonsoft.Json;
using System;
using System.Reflection;

namespace Minor.WSA.Infrastructure
{
    public class CommandHandler : ICommandHandler
    {
        public IFactory Factory { get; }
        public MethodInfo Method { get; }
        public Type ReturnType { get; }
        public Type ParamType { get; }

        public CommandHandler(IFactory factory, MethodInfo method, Type returnType, Type paramType)
        {
            Factory = factory;
            Method = method;
            ReturnType = returnType;
            ParamType = paramType;
        }

        public CommandResultMessage DispatchCommand(CommandReceivedMessage commandReceivedMessage)
        {
            var paramObj = JsonConvert.DeserializeObject(commandReceivedMessage.JsonMessage, ParamType);
            var instance = Factory.GetInstance();

            try
            {
                var result = Method.Invoke(instance, new object[] { paramObj });

                var resultType = ReturnType.ToString();
                var resultJson = JsonConvert.SerializeObject(result);
                return new CommandResultMessage(resultType, resultJson);

            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException;
            }
        }
    }
}