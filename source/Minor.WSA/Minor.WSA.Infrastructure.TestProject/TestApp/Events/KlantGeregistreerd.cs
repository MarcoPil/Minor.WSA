using Minor.WSA.Common;

namespace MVM.Polisbeheer.Klantbeheer.Events
{
    public class KlantGeregistreerd : DomainEvent
    {
        public KlantGeregistreerd() : base("MVM.Klantbeheer.KlantGeregistreerd")
        {
        }
    }
}