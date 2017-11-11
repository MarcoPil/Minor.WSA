using System;
using System.Runtime.Serialization;

namespace Minor.WSA.Infrastructure
{
    public class MicroserviceException : Exception
    {
        public int Code { get; }

        public MicroserviceException(string message) : base(message)
        {
        }
        public MicroserviceException(int code, string message) : base(message)
        {
            Code = code;
        }

        public MicroserviceException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}