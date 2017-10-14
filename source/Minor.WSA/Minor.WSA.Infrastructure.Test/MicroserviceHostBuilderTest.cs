using Minor.WSA.Infrastructure;
using Minor.WSA.Infrastructure.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using System.Threading;

public class MicroserviceHostBuilderTest
{
    [Fact]
    public void BuilderCreatesHost()
    {
        var target = new MicroserviceHostBuilder();

        var result = target.CreateHost();

        Assert.NotNull(result);
    }

    [Fact]
    public void BuilderAddEventHandler()
    {
        var target = new MicroserviceHostBuilder();

        MicroserviceHostBuilder result = target.AddEventHandler<AnotherEventHandler>();

        Assert.Equal(1, result.Factories.Count());
        Assert.Contains("Minor.WSA.Infrastructure.Test.AnotherEventHandler", result.Factories);
        var routingkeys = result.EventListeners.First().TopicExpressions;
        Assert.Equal(2, routingkeys.Count());
        Assert.Contains("#.Another.SomeEvent", routingkeys);
        Assert.Contains("WSA.Test.OtherEvent", routingkeys);
    }

    [Fact]
    public void IncorrectRoutingkey()
    {
        var target = new MicroserviceHostBuilder();

        Action action = () =>
        {
            MicroserviceHostBuilder result = target.AddEventHandler<IncorrectRoutingKeyEventHandler>();
        };

        var ex = Assert.Throws<MicroserviceConfigurationException>(action);
        Assert.Equal("Routingkey Expression '#OtherEvent' has an invalid expression format.", ex.Message);
    }

    [Fact]
    public void HostBuilderSetsBusOptions()
    {
        var target = new MicroserviceHostBuilder();
        var options = new BusOptions("HostBuilderSetsBusOptions_Ex01");

        target.WithBusOptions(options);

        using (var host = target.CreateHost())
        {
            Assert.Equal(options, host.BusOptions);
        }
    }

    [Fact]
    public void DoDependencyInjection()
    {
        var busOptions = new BusOptions(exchangeName: "DoDependencyInjectionTest_Ex02");

        var target = new MicroserviceHostBuilder()
            .WithBusOptions(busOptions)
            .AddEventHandler<DiTestEventHandler>();
        DiTest.ContructionCount = 0;
        target.ServiceProvider.AddTransient<IDiTest, DiTest>();

        using (var host = target.CreateHost())
        using (var publisher = new EventPublisher(busOptions))
        {
            host.StartListening();
            host.StartHandling();

            var evt = new SomeEvent();
            publisher.Publish(evt); // publish the event once
            publisher.Publish(evt); // publish the event once
            Thread.Sleep(100);

            Assert.Equal(2, DiTestEventHandler.GlobalCallCount);
            Assert.Equal(2, DiTest.ContructionCount);
        }
    }
}

public interface IDiTest { }
public class DiTest : IDiTest
{
    public static int ContructionCount = 0;
    
    public DiTest()
    {
        ContructionCount++;
    }
}

[EventListener("Unittest.WSA.DiTest")]
public class DiTestEventHandler
{
    public IDiTest _injected = null;
    public static int GlobalCallCount = 0;

    public DiTestEventHandler(IDiTest injectable)
    {
        _injected = injectable;
    }

    [Topic("Test.WSA.SomeEvent")]
    public void Handle(SomeEvent evt)
    {
        GlobalCallCount++;
    }
}
