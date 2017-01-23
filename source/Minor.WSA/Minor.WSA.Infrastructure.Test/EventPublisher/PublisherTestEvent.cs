using Minor.WSA.Common;

internal class PublisherTestEvent : DomainEvent
{
    public PublisherTestEvent() : base("Minor.WSA.PublisherTestEvent")
    {
    }
}