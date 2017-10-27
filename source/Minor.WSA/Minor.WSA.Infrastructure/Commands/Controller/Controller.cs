using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Minor.WSA.Infrastructure
{
    public class Controller : IController
    {
        private Dictionary<string, ICommandHandler> _commandHandlers; //    string = commandName
        public string QueueName { get; }
        public IEnumerable<KeyValuePair<string, ICommandHandler>> Commands => _commandHandlers;
        public BusOptions BusOptions { get; private set; }

        public Controller(string queueName, Dictionary<string, ICommandHandler> commandHandlers)
        {
            QueueName = queueName;
            _commandHandlers = commandHandlers;
        }

        public void OpenCommandQueue(BusOptions busOptions)
        {
            BusOptions = busOptions;
            busOptions.Provider.CreateCommandQueue(QueueName);
        }

        public void StartHandling()
        {
            BusOptions.Provider.StartReceivingCommands(QueueName, CommandReceived);
        }

        protected virtual CommandResultMessage CommandReceived(CommandReceivedMessage commandReceivedMessage)
        {
            CommandResultMessage result;
            var commandType = commandReceivedMessage.CommandType;

            if (_commandHandlers.ContainsKey(commandType))
            {
                try
                {
                    result = _commandHandlers[commandType].DispatchCommand(commandReceivedMessage);
                }
                catch (FunctionalException ex)
                {
                    var resultJson = JsonConvert.SerializeObject(ex.ErrorList);
                    result = new CommandResultMessage("FunctionalException", resultJson);
                }
                catch
                {
                    var error = new TechnicalError(501, $"Internal Server Error");
                    result = new CommandResultMessage("TechnicalError", JsonConvert.SerializeObject(error));
                }
            }
            else
            {
                var error = new TechnicalError(404, $"Cannot Execute '{commandType}'. Command not found.");
                result = new CommandResultMessage("TechnicalError", JsonConvert.SerializeObject(error));
            }

            return result;
        }
    }
}