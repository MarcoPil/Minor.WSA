using Minor.WSA.Common;
using Minor.WSA.Infrastructure.Shared.TestBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Xunit;

namespace Minor.WSA.Infrastructure.Test
{
    public class GenericEventHandlerTest
    {
        [Fact]
        public void TopicOfGenericHandlerIsHash()
        {
            var target = new MicroserviceHostBuilder()
                .AddEventListener<GenericEventListener>();
            Assert.Contains("#", target.EventListeners.First().TopicExpressions);
        }

        [Fact]
        public void CanHaveTwoGenericHandlers()
        {
            var target = new MicroserviceHostBuilder()
                .AddEventListener<GenericEventListener>();
            Assert.Contains("#", target.EventListeners.First().TopicExpressions);
            Assert.Contains("More.*.Specific", target.EventListeners.First().TopicExpressions);
        }
        #region Test Dummies for TopicOfGenericHandlerIsHash & CanHaveTwoGenericHandlers 
        [EventListener("GenericEventListener.TestQueue")]
        private class GenericEventListener
        {
            public void GenericHandler(EventMessage eventMessage)
            {
            }

            [Topic("More.*.Specific")]
            public void SemiGenericHandler(EventMessage eventMessage)
            {
            }
        }
        #endregion Test Dummies for TopicOfGenericHandlerIsHash & CanHaveTwoGenericHandlers 

        [Fact]
        public void CannotHaveTwoIdenticalGenericTopicExpressions()
        {
            var target = new MicroserviceHostBuilder();

            Action action = () => target.AddEventListener<InvalidGenericEventListener>();

            var ex = Assert.Throws<MicroserviceConfigurationException>(action);
            Assert.Equal("Two topic expressions cannot be exactly identical. The topic expression '#' has already been registered.", ex.Message);
        }
        #region CannotHaveTwoIdenticalGenericTopicExpressions Test Dummies
        [EventListener("GenericEventListener.TestQueue")]
        private class InvalidGenericEventListener
        {
            public void GenericHandler(EventMessage eventMessage)
            {
            }
            public void AnotherGenericHandler(EventMessage eventMessage)
            {
            }
        }
        #endregion CannotHaveTwoIdenticalGenericTopicExpressions Test Dummies

        [Fact]
        public void GenericHandlerReceivesMessage()
        {
            var options = new TestBusOptions();
            var builder = new MicroserviceHostBuilder()
                .WithBusOptions(options)
                .AddEventListener<ReceivingGenericEventListener>();
            using (var host = builder.CreateHost())
            {
                host.StartListening();
                host.StartHandling();
                using (var publisher = new EventPublisher(options))
                {
                    publisher.Publish(new NonGenericEvent());
                }
            }
            Thread.Sleep(200);
            Assert.Equal(1, ReceivingGenericEventListener.CallCount);
        }
        #region Test Dummies for ReceivingGenericEventListener  
        [EventListener("GenericEventListener.TestQueue")]
        private class ReceivingGenericEventListener
        {
            public static int CallCount = 0;
            public void GenericHandler(EventMessage eventMessage)
            {
                CallCount++;
            }
        }

        private class NonGenericEvent : DomainEvent
        {
            public NonGenericEvent() : base("Test.WSA.NonGenericEvent")
            {
            }
            public int Number { get; set; }
        }
        #endregion
    }
}
