namespace Minor.WSA.Infrastructure
{
    public interface IBusOptions
    {
        string ExchangeName { get; }
        string HostName { get; }
        int Port { get; }
        string UserName { get; }
        string Password { get; }
        IBusProvider Provider { get; }

        BusOptions CopyWith(string exchangeName = null, string hostName = null, int? port = null, string userName = null, string password = null);
        string ToString();
    }
}