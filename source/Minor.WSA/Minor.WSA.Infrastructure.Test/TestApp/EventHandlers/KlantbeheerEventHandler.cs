﻿using Minor.WSA.Infrastructure.Test.TestApp.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Minor.WSA.Infrastructure.Test.TestApp.EventHandlers
{
    // [EventHandler(RoutingKey="MVM.Klantbeheer.*")]  
    /// default RoutingKey = "#.Klantbeheer.#"  ("#.<<bla>>.#", want de class heet <bla>>EventHandler)
    public class KlantbeheerEventHandler
    {
        /// Default RoutingKey = "#.Klantbeheer.KlantGeregistreerd" = "#.NaamEventHander.NaamEvent"
        public void KlantGeregistreerd(KlantGeregistreerd evt)
        {

        }

        // [RoutingKey("MVM.Klantbeheer.KlantVerhuisdEvent")]    /// Reageert op "MVM.Klantbeheer.KlantVerhuisdEvent"-event en bindt dit aan KlantVerhuisd-event
        public void KlantVerhuisd(KlantVerhuisd evt)
        {

        }

    }
}
