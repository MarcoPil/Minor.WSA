using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Minor.WSA.Infrastructure.Test
{
    public class RabbitTestHelp
    {
        public static ConnectionFactory CreateFactoryFrom(BusOptions busOptions)
        {
            return new ConnectionFactory()
            {
                HostName = busOptions.HostName,
                Port = busOptions.Port,
                UserName = busOptions.UserName,
                Password = busOptions.Password,
            };
        }

        public static void DeleteExchange(BusOptions busOptions)
        {
            using (var connection = CreateFactoryFrom(busOptions).CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeleteNoWait(busOptions.ExchangeName);
            }
        }

        internal static void DeleteQueueAndExchange(BusOptions busOptions, string queueName)
        {
            using (var connection = CreateFactoryFrom(busOptions).CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeleteNoWait(queueName);
                channel.ExchangeDeleteNoWait(busOptions.ExchangeName);
            }
        }
    }
}
