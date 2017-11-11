using Minor.WSA.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Minor.WSA.Infrastructure
{
    public class Commander : IDisposable, ICommander
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

        public async Task ExecuteAsync(string serviceName, DomainCommand command)
        {
            await ExecuteCommand(serviceName, command);
        }
        public async Task<T> ExecuteAsync<T>(string serviceName, DomainCommand command)
        {
            CommandResponseMessage commandResponseMessage = await ExecuteCommand(serviceName, command);

            T result = JsonConvert.DeserializeObject<T>(commandResponseMessage.JsonMessage);
            return result;
        }

        private async Task<CommandResponseMessage> ExecuteCommand(string serviceName, DomainCommand command)
        {
            var commandRequestMessage = new CommandRequestMessage(
                serviceQueueName: serviceName,
                commandType: command.GetType().FullName,
                jsonMessage: JsonConvert.SerializeObject(command)
            );

            var commandResponseMessage = await BusOptions.Provider.SendCommandAsync(commandRequestMessage);

            if (commandResponseMessage.Type == "FunctionalException")
            {
                Error[] errorList = JsonConvert.DeserializeObject<Error[]>(commandResponseMessage.JsonMessage);
                throw new FunctionalException(errorList);
            }
            else if (commandResponseMessage.Type == "TechnicalError")
            {
                TechnicalError error = JsonConvert.DeserializeObject<TechnicalError>(commandResponseMessage.JsonMessage);
                throw new MicroserviceException(error.Code, error.Message);
            }
            else
            {
                return commandResponseMessage;
            }
        }
    }
}