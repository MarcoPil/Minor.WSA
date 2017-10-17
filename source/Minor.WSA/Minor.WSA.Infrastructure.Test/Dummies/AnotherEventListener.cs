namespace Minor.WSA.Infrastructure.Test
{
    [EventListener("Unittest.WSA.Test")]
    internal class AnotherEventListener
    {
        public void Handle(SomeEvent evt)
        {
        }

        [Topic("WSA.Test.OtherEvent")]
        public void Handle(OtherEvent evt)
        {

        }
    }
}