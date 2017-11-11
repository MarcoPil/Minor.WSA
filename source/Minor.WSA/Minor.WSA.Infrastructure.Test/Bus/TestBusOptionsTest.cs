using Minor.WSA.Common;
using Minor.WSA.Infrastructure;
using Minor.WSA.Infrastructure.Test;
using Minor.WSA.Infrastructure.TestBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

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
            Assert.Contains(options.LoggedEventMessages, m => m.EventType == typeof(DE01).FullName);
        }
    }

    [Fact]
    public void CommandMessagesAreLogged()
    {
        var busOptions = new TestBusOptions();
        var builder = new MicroserviceHostBuilder()
            .AddController<Test2Controller>()
            .WithBusOptions(busOptions);

        using (var host = builder.CreateHost())
        {
            host.Start();

            using (var target = new Commander(busOptions))
            {
                // Act
                var command = new Test2Command() { Name = "Karina" };
                var resultTask = target.ExecuteAsync<Test2Result>("Test2Service", command);

                var received = resultTask.Wait(1000);
                Assert.True(received);
            }
        }
        Assert.Contains(busOptions.LoggedCommandRequestMessages, m => m.CommandType == typeof(Test2Command).FullName);
        Assert.Contains(busOptions.LoggedCommandResultMessages, m => m.Type == typeof(Test2Result).FullName);
    }
    #region dummies
    private class Test2Command : DomainCommand
    {
        public string Name { get; set; }
    }
    private class Test2Result
    {
        public string Greeting { get; set; }
    }

    [Controller("Test2Service")]
    private class Test2Controller
    {
        public Test2Result Execute(Test2Command command)
        {
            return new Test2Result { Greeting = "Hello, " + command.Name };
        }
    }
    #endregion dummies
}
