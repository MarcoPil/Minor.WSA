using System;
using System.Runtime.Serialization;

namespace Minor.WSA.Infrastructure
{
    public class MicroserviceException : Exception
    {
        public MicroserviceException(string message) : base(message)
        {
        }

        public MicroserviceException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}