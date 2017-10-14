namespace Minor.WSA.Infrastructure.Test
{
    [EventHandler("Unittest.WSA.Test")]
    internal class AnotherEventHandler
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