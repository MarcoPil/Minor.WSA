using Minor.WSA.Common;
using Minor.WSA.Infrastructure.Shared.TestBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace Minor.WSA.Infrastructure.Test
{
    public class TestBusOptionsTest
    {
        [Fact]
        public void TestBusTransfersEventMessages()
        {
            BusOptions options = new TestBusOptions();

            var builder = new MicroserviceHostBuilder()
                .WithBusOptions(options)
                .AddEventListener<EL01>();

            using (var publisher = new EventPublisher(options))
            using (var host = builder.CreateHost())
            {
                host.StartListening();
                host.StartHandling();
                EL01.HandlerCallCount = 0;

                // Act
                publisher.Publish(new DE01());

                // Assert
                Assert.Equal(1, EL01.HandlerCallCount);
            }
        }
        #region TestBusTransfersEventMessages classes
        [EventListener("EL01queue")]
        private class EL01
        {
            public static int HandlerCallCount = 0;

            [Topic("Minor.WSA.DE01")]
            public void Handler(DE01 de01)
            {
                HandlerCallCount++;
            }
        }

        private class DE01 : DomainEvent
        {
            public DE01() : base("Minor.WSA.DE01")
            {
            }
        }
        #endregion TestBusTransfersEventMessages classes

        [Fact]
        public void PublishedMessagesAreLogged()
        {
            var options = new TestBusOptions();
            using (var publisher = new EventPublisher(options))
            {
                // Act
                publisher.Publish(new DE01());

                // Assert
                Assert.Contains(options.LoggedMessages, m => m.EventType == typeof(DE01).FullName);
            }
        }
    }
}
