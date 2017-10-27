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
    public void BuilderAddEventListener()
    {
        var target = new MicroserviceHostBuilder();

        MicroserviceHostBuilder result = target.AddEventListener<AnotherEventListener>();

        Assert.Equal(1, result.Factories.Count());
        Assert.Contains("Minor.WSA.Infrastructure.Test.AnotherEventListener", result.Factories);
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
            MicroserviceHostBuilder result = target.AddEventListener<IncorrectTopicEventListener>();
        };

        var ex = Assert.Throws<MicroserviceConfigurationException>(action);
        Assert.Equal("Topic Expression '#OtherEvent' has an invalid expression format.", ex.Message);
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
            .AddEventListener<DiTestEventListener>();
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

            Assert.Equal(2, DiTestEventListener.GlobalCallCountHandle);
            Assert.Equal(2, DiTest.ContructionCount);
        }
        RabbitTestHelp.DeleteQueueAndExchange(busOptions, "Unittest.WSA.DiTest");
    }
    #region DoDependencyInjection Test Dummies
    private interface IDiTest { }
    private class DiTest : IDiTest
    {
        public static int ContructionCount = 0;

        public DiTest()
        {
            ContructionCount++;
        }
    }

    [EventListener("Unittest.WSA.DiTest")]
    private class DiTestEventListener
    {
        public IDiTest _injected = null;
        public static int GlobalCallCountHandle = 0;
        public static int GlobalCallCountOtherHandle = 0;

        public DiTestEventListener(IDiTest injectable)
        {
            _injected = injectable;
        }

        [Topic("Test.WSA.SomeEvent")]
        public void Handle(SomeEvent evt)
        {
            GlobalCallCountHandle++;
        }
    }
    #endregion DoDependencyInjection Test Dummies

    [Fact]
    public void CannotHaveTwoIdenticalTopicExpressions()
    {
        var target = new MicroserviceHostBuilder();
        
        Action action = () => target.AddEventListener<InvalidDuplicateTestEventListener>();

        var ex = Assert.Throws<MicroserviceConfigurationException>(action);
        Assert.Equal("Two topic expressions cannot be exactly identical. The topic expression 'Test.WSA.SomeEvent' has already been registered.", ex.Message);
    }
    #region CannotHaveTwoIdenticalTopicExpressions Test Dummies
    [EventListener("Unittest.WSA.InvalidDuplicateTest")]
    private class InvalidDuplicateTestEventListener
    {
        [Topic("Test.WSA.SomeEvent")]
        public void Handle(SomeEvent evt)
        {
        }
        [Topic("Test.WSA.SomeEvent")]
        public void OtherHandle(SomeEvent evt)
        {
        }
    }
    #endregion CannotHaveTwoIdenticalTopicExpressions Test Dummies

    [Fact]
    public void EventListenerCanHaveOtherMethods()
    {
        var target = new MicroserviceHostBuilder();

        target.AddEventListener<EventListenerWithOtherMethods>();

        Assert.Equal(1, target.EventListeners.Count());
    }
    #region EventListenerCanHaveOtherMethods Test Dummies
    [EventListener("Unittest.WSA.EventListenerWithOtherMethods")]
    private class EventListenerWithOtherMethods
    {
        public void GenericHandler(EventMessage eventMessage)
        {
        }
        public void NotAnHandler(int a, Boolean b)
        {
        }
    }
    #endregion EventListenerCanHaveOtherMethods Test Dummies

    [Fact]
    public void TopicCannotBeNull()
    {
        var target = new MicroserviceHostBuilder();

        Action action = () => target.AddEventListener<EventListenerWithNullTopic>();

        var ex = Assert.Throws<MicroserviceConfigurationException>(action);
        Assert.Equal("Topic Expression '' has an invalid expression format.", ex.Message);
    }
    #region TopicCannotBeNull Test Dummies
    [EventListener("Unittest.WSA.EventListenerWithOtherMethods")]
    private class EventListenerWithNullTopic
    {
        [Topic(null)]
        public void GenericHandler(EventMessage eventMessage)
        {
        }
    }
    #endregion TopicCannotBeNull Test Dummies

}


