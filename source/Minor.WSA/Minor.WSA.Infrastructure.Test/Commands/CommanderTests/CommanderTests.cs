using Minor.WSA.Common;
using Minor.WSA.Infrastructure;
using Minor.WSA.Infrastructure.Shared.TestBus;
using Minor.WSA.Infrastructure.Test;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

public class CommanderTests
{
    [Fact]
    public void DefaultBusoptions()
    {
        using (var target = new Commander(default(BusOptions)))
        {
            var result = target.BusOptions;

            Assert.Equal("WSA.DefaultEventBus", result.ExchangeName);
            Assert.Equal("localhost", result.HostName);
            Assert.Equal(5672, result.Port);
            Assert.Equal("guest", result.UserName);
            Assert.Equal("guest", result.Password);
        }
        RabbitTestHelp.DeleteExchange(new BusOptions());
    }

    [Fact]
    public void ExecuteCommand()
    {
        var busOptions = new TestBusOptions();
        using (var target = new Commander(busOptions))
        {
            var command = new Test1Command();
            var resultTask = target.ExecuteAsync<string>("MyServiceName", command);
        }
        var loggedCommands = (busOptions.Provider as TestBusProvider).LoggedCommandRequestMessages;
        Assert.Contains(loggedCommands, c => c.CommandType == typeof(Test1Command).FullName);
    }
    #region dummies
    private class Test1Command : DomainCommand
    {
    }
    #endregion dummies


    [Fact]
    public void ExecuteCommandwithResult()
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
        RabbitTestHelp.DeleteQueueAndExchange(busOptions, "Test2Service");
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