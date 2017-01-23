using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Minor.WSA.Infrastructure
{
    public abstract class EventBusBase : IDisposable
    {
        private IConnection _connection;
        protected IModel Channel;
        public BusOptions BusOptions { get; }

        public EventBusBase(BusOptions options = default(BusOptions))
        {
            BusOptions = options ?? new BusOptions();
        }

        public virtual void Open()
        { 
            var factory = new ConnectionFactory()
            {
                HostName = BusOptions.HostName,
                Port =     BusOptions.Port,
                UserName = BusOptions.UserName,
                Password = BusOptions.Password,
            };
            try
            {
                _connection = factory.CreateConnection();
            }
            catch
            {
//                throw new MicroserviceConfigurationException("The Eventbus (RabbitMQ service) cannot be reached.");
            }
            Channel = _connection.CreateModel();

            Channel.ExchangeDeclare(exchange: BusOptions.ExchangeName,
                                    type: ExchangeType.Topic,
                                    durable: false, autoDelete: false, arguments: null);
        }

        public virtual void Dispose()
        {
            Channel?.Dispose();
            _connection?.Dispose();
        }
    }
}
