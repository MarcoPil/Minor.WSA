using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Minor.WSA.AuditLog.DAL;
using Minor.WSA.Infrastructure;
using System;
using System.Linq;

namespace Minor.WSA.AuditLog
{
    class Program
    {
        static void Main(string[] args)
        {
            var busOptions = BusOptions.CreateFromEnvironment();
            var connectionString = Environment.GetEnvironmentVariable("AuditLogDb") ??
                @"Server=.\SQLEXPRESS;Database=AuditLogDb_Test;Integrated security=true";

            var builder = new MicroserviceHostBuilder()
                .UseConventions()
                .WithBusOptions(busOptions);
            builder.ServiceProvider.AddDbContext<LoggerContext>(options =>
                options.UseSqlServer(connectionString));
            builder.ServiceProvider.AddTransient<ILogRepository,LogRepository>();

            using (var host = builder.CreateHost())
            {
                host.Start();

                Console.WriteLine($"Auditlog is listening...");
                Console.WriteLine(busOptions.ToString());
                foreach (var controller in host.Controllers)
                {
                    Console.WriteLine($"On Queue \"{controller.QueueName}\" for commands:");
                    foreach (var commandPair in controller.Commands)
                    {
                        Console.WriteLine($"\t{commandPair.Key}");
                    }
                }
                foreach (var eventListener in host.EventListeners)
                {
                    Console.WriteLine($"On Queue \"{eventListener.QueueName}\" for all events that match one of the following topics:");
                    foreach (var topic in eventListener.TopicExpressions)
                    {
                        Console.WriteLine($"\t{topic}");
                    }
                }
                Console.WriteLine();
                Console.WriteLine("Press any key to quit.");

                Console.ReadKey();
            }
        }
    }
}
