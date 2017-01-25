using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace Minor.WSA.Infrastructure.Test.EventHandlerTests
{
    public class EventDispatcherTest
    {
        [Fact]
        public void DispatchesEvent()
        {
            // Arrange
            var testHandler = new DispatcherTestMock();

            var factoryMock = new Mock<Factory>();
            factoryMock.Setup(fm => fm.GetInstance())
                       .Returns(testHandler);
            var factory = factoryMock.Object;

            var method = typeof(DispatcherTestMock).GetMethod("HandleDispatchTestEvent");
            var paramType = typeof(DispatchTestEvent);

            var target = new EventDispatcher(factory, method, paramType);

            // Act
            var sentEvent = new DispatchTestEvent { Number = 7 };
            string jsonMessage = JsonConvert.SerializeObject(sentEvent);
            target.DispatchEvent(jsonMessage);

            // Assert
            Assert.NotNull(testHandler.EventReceived);
        }


    }
}
