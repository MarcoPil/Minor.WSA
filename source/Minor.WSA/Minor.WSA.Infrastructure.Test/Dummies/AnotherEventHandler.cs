namespace Minor.WSA.Infrastructure.Test
{
    [EventHandler]
    internal class AnotherEventHandler
    {
        public void Handle(SomeEvent evt)
        {
        }

        [RoutingKey("WSA.Test.OtherEvent")]
        public void Handle(OtherEvent evt)
        {

        }
    }
}