using System.Collections.Generic;

namespace Minor.WSA.Infrastructure
{
    public interface IController
    {
        IEnumerable<KeyValuePair<string, ICommandHandler>> Commands { get; }
        string QueueName { get; }

        void OpenCommandQueue(BusOptions busOptions);
        void StartHandling();
    }
}