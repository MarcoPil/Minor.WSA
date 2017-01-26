using Minor.WSA.Common;

namespace Minor.WSA.Infrastructure.Test
{
    public class OtherEvent : DomainEvent
    {
        public OtherEvent() : base("Test.WSA.OtherEvent")
        {
        }
    }
}