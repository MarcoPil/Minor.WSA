using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Minor.WSA.Infrastructure.Test.TestApp
{
    public class PolisbeheerProgram
    {
        public static void Main(string[] args)
        {
            var host = new MicroserviceHostBuilder()
                            //.Configure()      // Read environmentVariables, initialize Dependency Injection
                            .UseConventions()     // Find Handers and Controllers using reflection
                            //.AddEventHandler<MyEventHandler>()    // Explicitly add EventHandler
                            //.AddController<MyController>()        // Explicitly add Controller
                            //.EnableLogging(LogLevel.Debug)
                            //.CreateSwaggerEndpoint()     // Expose Metadata in Swagger-format
                            //.DelayStartup(optinalEnvironmentVarName);    // EnviromnentVarName defaults to startupDelayInSeconds, delaytime defaults to 0
                            .CreateHost();

            //host.Open(); // Opens RabbitMQ connections and queues (the queues receive messages, the host only after start)

            //host.StartAfterReplay(optionalEnvironmentVarAuditlogEndpointName);
            //host.Start();
        }
    }
}
