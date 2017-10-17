using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public void CannotHaveTwoIdenticalGenericTopicExpressions()
        {
            var target = new MicroserviceHostBuilder();

            Action action = () => target.AddEventListener<InvalidGenericEventListener>();

            var ex = Assert.Throws<MicroserviceConfigurationException>(action);
            Assert.Equal("Two topic expressions cannot be exactly identical. The topic expression '#' has already been registered.", ex.Message);
        }

        [Fact]
        public void CanHaveTwoGenericHandlers()
        {
            var target = new MicroserviceHostBuilder()
                .AddEventListener<GenericEventListener>();
            Assert.Contains("#", target.EventListeners.First().TopicExpressions);
            Assert.Contains("More.*.Specific", target.EventListeners.First().TopicExpressions);
        }

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
    }
}
