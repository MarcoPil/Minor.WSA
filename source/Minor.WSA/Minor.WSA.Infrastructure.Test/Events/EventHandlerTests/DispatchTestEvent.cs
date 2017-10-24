using Minor.WSA.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Minor.WSA.Infrastructure.Test.EventHandlerTests
{
    public class DispatchTestEvent : DomainEvent
    {
        public DispatchTestEvent() : base("MVM.Test.DispatchTest")
        {
        }

        public int Number { get; set; }
    }
}
