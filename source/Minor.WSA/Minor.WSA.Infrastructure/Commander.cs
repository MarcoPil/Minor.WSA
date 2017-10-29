using Minor.WSA.Common;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Minor.WSA.Infrastructure
{
    public class Commander : IDisposable
    {
        public BusOptions BusOptions { get; }

        public Commander(BusOptions busOptions)
        {
            BusOptions = busOptions ?? new BusOptions();
        }

        public void Dispose()
        {
            BusOptions.Dispose();
        }

        public async Task<T> ExecuteAsync<T>(string serviceName, DomainCommand command)
        {
            var commandRequestMessage = new CommandRequestMessage(
                serviceQueueName: serviceName,
                commandType: command.GetType().FullName,
                jsonMessage: JsonConvert.SerializeObject(command)
            );

            var commandResultMessage = await BusOptions.Provider.SendCommandAsync(commandRequestMessage);

            T result = JsonConvert.DeserializeObject<T>(commandResultMessage.JsonMessage);
            return result;
        }
    }
}