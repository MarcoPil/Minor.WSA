
namespace Minor.WSA.Infrastructure.Test
{
    [EventHandler("Incorrect.Routingkey")]
    internal class IncorrectRoutingKeyEventHandler
    {
        [Topic("#OtherEvent")]
        public void Handle(SomeEvent evt)
        {
        }
    }
}