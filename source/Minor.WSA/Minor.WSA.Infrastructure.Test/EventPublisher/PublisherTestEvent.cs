using Minor.WSA.Common;

namespace Minor.WSA.Infrastructure.Test
{

    internal class PublisherTestEvent : DomainEvent
    {
        public PublisherTestEvent() : base("Minor.WSA.PublisherTestEvent")
        {
        }
    }
}