using Minor.WSA.Infrastructure.Test.TestApp.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Minor.WSA.Infrastructure.Test.TestApp.EventHandlers
{
    [EventHandler("MVM.Polisbeheer.KlantbeheerEvents")]
    public class KlantbeheerEventHandler
    {
        /// Default RoutingKey = "#.Klantbeheer.KlantGeregistreerd" = "#.NaamEventHander.NaamEvent"
        public void KlantGeregistreerd(KlantGeregistreerd evt)
        {

        }

        [RoutingKey("Test.WSA.KlantVerhuisd")]    /// Reageert op "Test.WSA.KlantVerhuisd"-event en bindt dit aan KlantVerhuisd-event
        public void KlantVerhuisd(KlantVerhuisd evt)
        {

        }

    }
}
