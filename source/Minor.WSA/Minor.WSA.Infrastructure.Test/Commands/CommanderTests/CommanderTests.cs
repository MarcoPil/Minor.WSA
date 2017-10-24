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
            //target.Execute(command);
        }
        Assert.True(false);
    }

    private class Test1Command
    {

    }
}