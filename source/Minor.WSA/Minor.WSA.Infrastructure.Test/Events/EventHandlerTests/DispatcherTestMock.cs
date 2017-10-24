using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Minor.WSA.Infrastructure.Test.EventHandlerTests
{
    public class DispatcherTestMock
    {
        public DispatchTestEvent EventReceived = null;
        public int ReceiveCount = 0;

        public void HandleDispatchTestEvent(DispatchTestEvent evt)
        {
            EventReceived = evt;
            ReceiveCount++;
        }

    }
}
