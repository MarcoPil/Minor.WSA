using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Minor.WSA.Infrastructure
{
    public abstract class EventBusBase : IDisposable
    {
        public BusOptions BusOptions { get; }

        public EventBusBase(BusOptions options = default(BusOptions))
        {
            BusOptions = options ?? new BusOptions();
        }

        protected virtual void CreateConnection()
        {
            BusOptions.Provider.CreateConnection();
        }


        public virtual void Dispose()
        {
            BusOptions.Provider.Dispose();
        }
    }
}
