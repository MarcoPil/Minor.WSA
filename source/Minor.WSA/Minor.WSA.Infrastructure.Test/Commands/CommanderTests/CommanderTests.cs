using Minor.WSA.Common;
using Minor.WSA.Infrastructure;
using Minor.WSA.Infrastructure.TestBus;
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
        var busOptions = new BusOptions();
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
                var result = resultTask.Result;
                Assert.Equal("Hello, Karina", result.Greeting);
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

    [Fact]
    public void ExecuteCommandwithFunctionalException()
    {
        var busOptions = new BusOptions();
        var builder = new MicroserviceHostBuilder()
            .AddController<Test3Controller>()
            .WithBusOptions(busOptions);

        using (var host = builder.CreateHost())
        {
            host.Start();

            using (var target = new Commander(busOptions))
            {
                // Act
                var command = new Test3Command() { Name = "Karina" };
                Action action = () =>
                {
                    var resultTask = target.ExecuteAsync<Test3Result>("Test3Service", command);
                    var result = resultTask.Result;
                };

                var aggregateException = Assert.Throws<AggregateException>( action );
                Assert.IsType<FunctionalException>(aggregateException.InnerException);
                FunctionalException fex = aggregateException.InnerException as FunctionalException;
                Assert.Contains(new Error("US023-1", "Don't drink and drive"), fex.ErrorList);
                Assert.Contains(new Error("US045-7b", "Don't put water on burning oil"), fex.ErrorList);
            }
        }
        RabbitTestHelp.DeleteQueueAndExchange(busOptions, "Test3Service");
    }
    #region dummies
    private class Test3Command : DomainCommand
    {
        public string Name { get; set; }
    }
    private class Test3Result
    {
        public string Greeting { get; set; }
    }

    [Controller("Test3Service")]
    private class Test3Controller
    {
        public Test3Result Execute(Test3Command command)
        {
            throw new FunctionalException(
                new Error("US023-1","Don't drink and drive"),
                new Error("US045-7b","Don't put water on burning oil")
            );
        }
    }
    #endregion dummies

    [Fact]
    public void ExecuteCommandwithTechnicalError()
    {
        var busOptions = new BusOptions();
        var builder = new MicroserviceHostBuilder()
            .AddController<Test4Controller>()
            .WithBusOptions(busOptions);

        using (var host = builder.CreateHost())
        {
            host.Start();

            using (var target = new Commander(busOptions))
            {
                // Act
                var command = new Test4Command();
                Action action = () =>
                {
                    var resultTask = target.ExecuteAsync<Test4Result>("Test4Service", command);
                    var result = resultTask.Result;
                };

                var aggregateException = Assert.Throws<AggregateException>(action);
                Assert.IsType<MicroserviceException>(aggregateException.InnerException);
                MicroserviceException ex = aggregateException.InnerException as MicroserviceException;
                Assert.Equal(501, ex.Code);
                Assert.Equal("Internal Server Error", ex.Message);
            }
        }
        RabbitTestHelp.DeleteQueueAndExchange(busOptions, "Test4Service");
    }
    #region dummies
    private class Test4Command : DomainCommand
    {
    }
    private class Test4Result
    {
    }

    [Controller("Test4Service")]
    private class Test4Controller
    {
        public Test4Result Execute(Test4Command command)
        {
            throw new ArgumentException("This is an outrageous argument!", nameof(command));
        }
    }
    #endregion dummies


    [Fact]
    public void ExecuteUnknownCommandWithReturnRaisesMicroServiceException()
    {
        var busOptions = new BusOptions();
        var builder = new MicroserviceHostBuilder()
            .AddController<Test5Controller>()
            .WithBusOptions(busOptions);

        using (var host = builder.CreateHost())
        {
            host.Start();

            using (var target = new Commander(busOptions))
            {
                // Act
                var command = new UnkwownCommand();
                Action action = () =>
                {
                    var resultTask = target.ExecuteAsync<Test5Result>("Test5Service", command);
                    var result = resultTask.Result;
                };

                var aggregateException = Assert.Throws<AggregateException>(action);
                Assert.IsType<MicroserviceException>(aggregateException.InnerException);
                MicroserviceException ex = aggregateException.InnerException as MicroserviceException;
                Assert.Equal(404, ex.Code);
                Assert.Equal($"Cannot Execute '{typeof(UnkwownCommand).ToString()}'. Command not found.", ex.Message);
            }
        }
        RabbitTestHelp.DeleteQueueAndExchange(busOptions, "Test5Service");
    }
    #region dummies
    private class UnkwownCommand : DomainCommand
    {
    }
    private class Test5Result
    {
    }

    [Controller("Test5Service")]
    private class Test5Controller
    {
    }
    #endregion dummies

    [Fact]
    public void ExecuteUnknownCommandRaisesMicroServiceException()
    {
        var busOptions = new BusOptions();
        var builder = new MicroserviceHostBuilder()
            .AddController<Test6Controller>()
            .WithBusOptions(busOptions);

        using (var host = builder.CreateHost())
        {
            host.Start();

            using (var target = new Commander(busOptions))
            {
                // Act
                var command = new UnkwownCommand6();
                Action action = () =>
                {
                    var task = target.ExecuteAsync("Test6Service", command);
                    bool executed = task.Wait(100);
                    Assert.True(executed);
                };

                var aggregateException = Assert.Throws<AggregateException>(action);
                Assert.IsType<MicroserviceException>(aggregateException.InnerException);
                MicroserviceException ex = aggregateException.InnerException as MicroserviceException;
                Assert.Equal(404, ex.Code);
                Assert.Equal($"Cannot Execute '{typeof(UnkwownCommand6).ToString()}'. Command not found.", ex.Message);
            }
        }
        RabbitTestHelp.DeleteQueueAndExchange(busOptions, "Test6Service");
    }
    #region dummies
    private class UnkwownCommand6 : DomainCommand
    {
    }
    private class Test6Result
    {
    }

    [Controller("Test6Service")]
    private class Test6Controller
    {
    }
    #endregion dummies


    [Fact]
    public void ExecuteCommandWithoutResult()
    {
        var busOptions = new BusOptions();
        var builder = new MicroserviceHostBuilder()
            .AddController<Test7Controller>()
            .WithBusOptions(busOptions);

        using (var host = builder.CreateHost())
        {
            host.Start();

            using (var target = new Commander(busOptions))
            {
                // Act
                var command = new Test7Command() { Name = "Karina" };
                var resultTask = target.ExecuteAsync<Test2Result>("Test7Service", command);

                var received = resultTask.Wait(100);
                Assert.True(received);
                Assert.Equal(1, Test7Controller.CallCount);
            }
        }
        RabbitTestHelp.DeleteQueueAndExchange(busOptions, "Test7Service");
    }
    #region dummies
    private class Test7Command : DomainCommand
    {
        public string Name { get; set; }
    }

    [Controller("Test7Service")]
    private class Test7Controller
    {
        public static int CallCount = 0;

        public void Execute(Test7Command command)
        {
            CallCount++;
        }
    }
    #endregion dummies
}