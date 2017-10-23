using System;
using System.Reflection;

namespace Minor.WSA.Infrastructure
{
    public interface ICommandHandler
    {
        IFactory Factory { get; }
        MethodInfo Method { get; }
        Type ReturnType { get; }
        Type ParamType { get; }

        CommandResultMessage DispatchCommand(CommandReceivedMessage commandReceivedMessage);
    }
}