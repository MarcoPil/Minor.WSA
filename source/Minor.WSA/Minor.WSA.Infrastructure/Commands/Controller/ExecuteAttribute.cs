using System;

namespace Minor.WSA.Infrastructure
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ExecuteAttribute : Attribute
    {
        public string CommandTypeName { get; }

        public ExecuteAttribute(string commandTypeName = null)
        {
            CommandTypeName = commandTypeName;
        }
    }
}