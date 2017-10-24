using System;

namespace Minor.WSA.Infrastructure
{
    public class MicroserviceConfigurationException : Exception
    {
        public MicroserviceConfigurationException(string message) : base(message)
        {
        }

        public MicroserviceConfigurationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}