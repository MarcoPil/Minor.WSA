using System.Collections.Generic;

namespace Minor.WSA.Infrastructure
{
    public class Controller : IController
    {
        private Dictionary<string, ICommandHandler> _commandHandlers; //    string = commandName
        public string QueueName { get; }
        public IEnumerable<string> Commands => _commandHandlers.Keys;
        public BusOptions BusOptions { get; private set; }

        public Controller(string queueName, Dictionary<string, ICommandHandler> commandHandlers)
        {
            QueueName = queueName;
            _commandHandlers = commandHandlers;
        }

        public void OpenCommandQueue(BusOptions busOptions)
        {
            BusOptions = busOptions;
            busOptions.Provider.CreateQueue(QueueName);
        }

        public void StartHandling()
        {
            throw new System.NotImplementedException();
        }
    }
}