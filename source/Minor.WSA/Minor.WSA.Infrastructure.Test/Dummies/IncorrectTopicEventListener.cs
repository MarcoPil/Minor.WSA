
namespace Minor.WSA.Infrastructure.Test
{
    [EventListener("Incorrect.Topic")]
    internal class IncorrectTopicEventListener
    {
        [Topic("#OtherEvent")]
        public void Handle(SomeEvent evt)
        {
        }
    }
}