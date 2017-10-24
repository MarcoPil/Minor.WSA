using System;

namespace Minor.WSA.Infrastructure
{
    public class Commander : IDisposable
    {
        public BusOptions BusOptions { get; }

        public Commander(BusOptions busOptions)
        {
            BusOptions = busOptions ?? new BusOptions();
        }

        public void Dispose()
        {
            BusOptions.Dispose();
        }
    }
}