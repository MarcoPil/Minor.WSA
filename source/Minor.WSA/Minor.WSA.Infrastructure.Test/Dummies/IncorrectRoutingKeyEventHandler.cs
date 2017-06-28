
namespace Minor.WSA.Infrastructure.Test
{
    [EventHandler("Incorrect.Routingkey")]
    internal class IncorrectRoutingKeyEventHandler
    {
        [RoutingKey("#OtherEvent")]
        public void Handle(SomeEvent evt)
        {
        }
    }
}