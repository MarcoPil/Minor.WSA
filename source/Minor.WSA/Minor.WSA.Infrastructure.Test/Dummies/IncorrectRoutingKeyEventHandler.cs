
namespace Minor.WSA.Infrastructure.Test
{
    [EventListener("Incorrect.Routingkey")]
    internal class IncorrectRoutingKeyEventHandler
    {
        [Topic("#OtherEvent")]
        public void Handle(SomeEvent evt)
        {
        }
    }
}