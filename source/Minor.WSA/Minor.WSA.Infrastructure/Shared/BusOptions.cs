using Minor.WSA.Infrastructure.Shared.TestBus;
using Newtonsoft.Json;
using System;

namespace Minor.WSA.Infrastructure
{
    /// <summary>
    /// The BusOptions are used for configuring a connection to RabbitMQ.
    /// </summary>
    public class BusOptions
    {
        /// <summary>
        /// Default: "WSA.DefaultEventBus"
        /// </summary>
        public string ExchangeName { get; private set; }
        /// <summary>
        /// Default HostName: "localhost"
        /// </summary>
        public string HostName { get; private set; }
        /// <summary>
        /// Default Port: 5672
        /// </summary>
        public int Port { get; private set; }
        /// <summary>
        /// Default UserName: "guest"
        /// </summary>
        public string UserName { get; private set; }
        /// <summary>
        /// Default Password: "guest"
        /// </summary>
        public string Password { get; private set; }
        /// <summary>
        /// EventBus Provider. Each Event Bus (RabbitMq, Kafka, TestBus) should have its own BusProvider
        /// </summary>
        [JsonIgnore]
        public IBusProvider Provider { get; private set; }

        /// <summary>
        /// Makes a copy of the Busoptions, replacing all provided parameters
        /// </summary>
        /// <param name="exchangeName">The exchange name for the copy. If null, the exchange name of the original will be used</param>
        /// <param name="hostName">The host name for the copy. If null, the host name of the original will be used</param>
        /// <param name="port">The port for the copy. If null, the port of the original will be used</param>
        /// <param name="userName">The user name for the copy. If null, the user name of the original will be used</param>
        /// <param name="password">The password for the copy. If null, the password of the original will be used</param>
        /// <returns></returns>
        public BusOptions CopyWith(string exchangeName = null, string hostName = null, int? port = null, string userName = null, string password = null)
        {
            return new BusOptions
            {
                ExchangeName = exchangeName ?? this.ExchangeName,
                HostName = hostName ?? this.HostName,
                Port = port ?? this.Port,
                UserName = userName ?? this.UserName,
                Password = password ?? this.Password,
            };
        }

        /// <summary>
        /// Initializes with default BusOptions
        /// </summary>
        public BusOptions(
                string exchangeName = "WSA.DefaultEventBus",
                string hostName = "localhost",
                int    port = 5672,
                string userName = "guest",
                string password = "guest")
        {
            ExchangeName = exchangeName;
            HostName = hostName;
            Port = port;
            UserName = userName;
            Password = password;
            Provider = new BusProvider(this);
        }

        /// <summary>
        /// Read BusOptions from environment variables:
        /// eventbus-exchangename, eventbus-hostname, eventbus-port, eventbus-username, eventbus-password
        /// </summary>
        /// <returns></returns>
        public static BusOptions CreateFromEnvironment()
        {
            var busOptions = new BusOptions();

            busOptions.ExchangeName = Environment.GetEnvironmentVariable("eventbus-exchangename") ?? busOptions.ExchangeName;
            busOptions.HostName = Environment.GetEnvironmentVariable("eventbus-hostname") ?? busOptions.HostName;
            int portnumber = 0;
            if (int.TryParse(Environment.GetEnvironmentVariable("eventbus-port"), out portnumber))
            {
                busOptions.Port = portnumber;
            }
            busOptions.UserName = Environment.GetEnvironmentVariable("eventbus-username") ?? busOptions.UserName;
            busOptions.Password = Environment.GetEnvironmentVariable("eventbus-password") ?? busOptions.Password;

            return busOptions;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}