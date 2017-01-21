using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Minor.WSA.Infrastructure.Test.TestApp.EventHandlers
{
    [EventHandler(RoutingKey="MVM.Klantbeheer.*")]  // default RoutingKey = "#.Klantbeheer.#"
    public class KlantbeheerEventHandler
    {
        public void KlantGeregistreerd(KlantGeregistreerd evt)
        {

        }
    }
}
