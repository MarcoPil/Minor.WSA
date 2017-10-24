namespace Minor.WSA.Infrastructure
{
    public interface IEventDispatcher
    {
        void DispatchEvent(EventMessage eventMessage);
    }
}