using System.Collections.Generic;

namespace Minor.WSA.Infrastructure
{
    public class Controller
    {
        private Dictionary<string, IEventDispatcher> _commandHandlers; //    string = commandName
        public string QueueName { get; }
        public IEnumerable<string> Commands => _commandHandlers.Keys;

        public Controller(string queueName, Dictionary<string, IEventDispatcher> commandHandlers)
        {
            QueueName = queueName;
            _commandHandlers = commandHandlers;
        }

    }
}