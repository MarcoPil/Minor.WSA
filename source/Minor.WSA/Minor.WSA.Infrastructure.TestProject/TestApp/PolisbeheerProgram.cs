using Minor.WSA.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
//using Microsoft.Extensions.Logging;

namespace MVM.Polisbeheer
{
    public class PolisbeheerProgram
    {
        private static EventWaitHandle waitForEnd = new AutoResetEvent(false);
        public static void aMain(string[] args)
        {
            var hostbuilder = new MicroserviceHostBuilder()
                            //.Configure()      // Read environmentVariables, initialize Dependency Injection
                            .UseConventions()     // Find Handers and Controllers using reflection
                            //.AddEventHandler<MyEventHandler>()    // Explicitly add EventHandler
                            //.AddController<MyController>()        // Explicitly add Controller
                            //.EnableLogging(LogLevel.Debug)
                            //.CreateSwaggerEndpoint()     // Expose Metadata in Swagger-format
                            //.DelayStartup(optinalEnvironmentVarName);    // EnviromnentVarName defaults to startupDelayInSeconds, delaytime defaults to 0
                            ;
            using (var host = hostbuilder.CreateHost())
            {
                host.StartListening(); // Opens RabbitMQ connections and queues (the queues receive messages, the host only after start)

                //host.StartAfterReplay(optionalEnvironmentVarAuditlogEndpointName);
                host.StartHandling();

                waitForEnd.WaitOne();
            }
        }

        public static void Stop()
        {
            waitForEnd.Set();
        }
    }
}
