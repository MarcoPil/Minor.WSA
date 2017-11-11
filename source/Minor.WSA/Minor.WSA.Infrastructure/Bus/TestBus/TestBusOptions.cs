using System;
using System.Collections.Generic;
using System.Text;

namespace Minor.WSA.Infrastructure.TestBus
{
    public class TestBusOptions : BusOptions
    {
        /// <summary>
        /// Logs all events that are published (sent by a EventPublisher)
        /// </summary>
        public IEnumerable<EventMessage> LoggedEventMessages { get; }
        /// <summary>
        /// Logs all commands that are sent (sent by a Commander)
        /// </summary>
        public IEnumerable<CommandRequestMessage> LoggedCommandRequestMessages { get; }
        /// <summary>
        /// Logs all replies to commands (sent by a Controller)
        /// </summary>
        public IEnumerable<CommandResultMessage> LoggedCommandResultMessages { get; }

        /// <summary>
        /// TestBusOptions provide the same functionality as BusOptions, except that the RabbitMQ connection is replaced by an in-memory EventBus.
        /// </summary>
        public TestBusOptions()
        {
            var testBusProvider = new TestBusProvider();
            Provider = testBusProvider;
            LoggedEventMessages = testBusProvider.LoggedEventMessages;
            LoggedCommandRequestMessages = testBusProvider.LoggedCommandRequestMessages;
            LoggedCommandResultMessages = testBusProvider.LoggedCommandResultMessages;
        }
    }
}
