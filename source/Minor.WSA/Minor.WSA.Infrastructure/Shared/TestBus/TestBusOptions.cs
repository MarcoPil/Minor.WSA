using System;
using System.Collections.Generic;
using System.Text;

namespace Minor.WSA.Infrastructure.Shared.TestBus
{
    public class TestBusOptions : BusOptions
    {
        public IEnumerable<EventMessage> LoggedMessages { get; }
        public TestBusOptions()
        {
            Provider = new TestBusProvider();
            LoggedMessages = (Provider as TestBusProvider).LoggedMessages;
        }
    }
}
