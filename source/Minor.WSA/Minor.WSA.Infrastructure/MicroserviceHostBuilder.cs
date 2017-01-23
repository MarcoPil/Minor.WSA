using System;

namespace Minor.WSA.Infrastructure
{
    public class MicroserviceHostBuilder
    {
        public MicroserviceHostBuilder()
        {
        }

        public MicroserviceHostBuilder UseConventions()
        {
            throw new NotImplementedException();
        }

        public MicroserviceHost CreateHost()
        {
            var host = new MicroserviceHost();

            return host;
        }

    }
}