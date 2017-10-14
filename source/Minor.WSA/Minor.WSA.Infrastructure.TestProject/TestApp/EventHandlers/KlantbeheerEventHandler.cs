using Minor.WSA.Infrastructure;
using MVM.Polisbeheer.Klantbeheer.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MVM.Polisbeheer.EventHandlers
{
    [EventListener("MVM.Polisbeheer.KlantbeheerEvents")]
    public class KlantbeheerEventHandler
    {
        /// Default Topic = "#.Klantbeheer.KlantGeregistreerd" = "#.NaamEventHander.NaamEvent"
        public void KlantGeregistreerd(KlantGeregistreerd evt)
        {

        }

        [Topic("MVM.Klantbeheer.KlantVerhuisd")]    /// Reageert op "MVM.Klantbeheer.KlantVerhuisd"-event en bindt dit aan MVM.Polisbeheer.KlantVerhuisd-event
        public void KlantVerhuisd(KlantVerhuisd evt)
        {

        }

    }
}
