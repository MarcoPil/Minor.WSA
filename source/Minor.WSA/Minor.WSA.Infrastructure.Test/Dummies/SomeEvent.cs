using Minor.WSA.Common;

namespace Minor.WSA.Infrastructure.Test
{
    public class SomeEvent : DomainEvent
    {
        public SomeEvent() : base("Test.WSA.SomeEvent")
        {
        }
    }
}