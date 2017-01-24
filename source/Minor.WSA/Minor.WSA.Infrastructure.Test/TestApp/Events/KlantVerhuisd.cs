using Minor.WSA.Common;

namespace Minor.WSA.Infrastructure.Test.TestApp.Events
{
    public class KlantVerhuisd : DomainEvent
    {
        public KlantVerhuisd() : base("Test.WSA.KlantVerhuisd")
        {
        }
    }
}