using Minor.WSA.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

public class MicroserviceHostBuilderControllerTests
{
    [Fact]
    public void AddControllerAddsController()
    {
        var target = new MicroserviceHostBuilder();

        target.AddController<Test1Controller>();

        Assert.Contains("MicroserviceHostBuilderControllerTests+Test1Controller", target.Factories);
        Assert.Single(target.Controllers);
        Assert.Contains(target.Controllers, c => c.QueueName == "Test1QueueName");
    }
    #region test dummies
    [Controller("Test1QueueName")]
    private class Test1Controller
    {

    }
    #endregion

    [Fact]
    public void AddControllerDoesNotAddNonController()
    {
        var target = new MicroserviceHostBuilder();

        target.AddController<NoController>();

        Assert.Empty(target.Controllers);
    }
    #region test dummies
    private class NoController
    {

    }
    #endregion

    [Fact]
    public void AddControllerFindcommandFromExecuteMethodName()
    {
        var target = new MicroserviceHostBuilder();

        target.AddController<Test2Controller>();

        var controller = target.Controllers.First();
        var handler = controller.Commands.First();
        Assert.Equal(typeof(Test2Command).ToString(), handler.Key);
        Assert.Equal(typeof(Test2Command), handler.Value.ParamType);
        Assert.Equal(typeof(string), handler.Value.ReturnType);
        Assert.Equal("Execute", handler.Value.Method.Name);
    }
    #region test dummies
    [Controller("Test2QueueName")]
    private class Test2Controller
    {
        public string Execute(Test2Command command)
        {
            return "result";
        }
    }
    public class Test2Command
    {
    }
    #endregion

    [Fact]
    public void AddControllerFindcommandsFromCommandAttribute()
    {
        var target = new MicroserviceHostBuilder();

        target.AddController<Test3Controller>();

        var controller = target.Controllers.First();
        Assert.Contains(controller.Commands, ch => ch.Key == typeof(Test3Command).ToString());
    }
    #region test dummies
    [Controller("Test3QueueName")]
    private class Test3Controller
    {
        [Execute]
        public string Handle(Test3Command command)
        {
            return "result";
        }
    }
    public class Test3Command
    {
    }
    #endregion

    [Fact]
    public void CanAddMultipleControllers()
    {
        var target = new MicroserviceHostBuilder();

        target.AddController<Test1Controller>()
              .AddController<Test2Controller>();

        Assert.Equal(2, target.Controllers.Count());
        Assert.Contains(typeof(Test1Controller).ToString(), target.Factories);
        Assert.Contains(typeof(Test2Controller).ToString(), target.Factories);
    }

    [Fact]
    public void CannotHaveTwoHandlersForSameCommand()
    {
        var target = new MicroserviceHostBuilder();

        Action action = () => target.AddController<Test4Controller>();

        var ex = Assert.Throws<MicroserviceConfigurationException>(action);
        var commandName = typeof(Test4Command).ToString();
        Assert.Equal($"Two commands cannot be exactly identical. The command '{commandName}' has already been registered.", ex.Message);
    }
    #region test dummies
    [Controller("Test4QueueName")]
    private class Test4Controller
    {
        public string Execute(Test4Command command)
        {
            return "result";
        }
        [Execute]
        public string Handle(Test4Command command)
        {
            return "result";
        }
    }
    public class Test4Command
    {
    }
    #endregion

    [Fact]
    public void AddControllerFromCommandWithAlternativeClass()
    {
        var target = new MicroserviceHostBuilder();

        target.AddController<Test5Controller>();

        var controller = target.Controllers.First();
        var handler = controller.Commands.First();
        Assert.Equal("TriggeredFromOtherCommand", handler.Key);
        Assert.Equal(typeof(Test5Command), handler.Value.ParamType);
        Assert.Equal(typeof(string), handler.Value.ReturnType);
        Assert.Equal("Handle", handler.Value.Method.Name);
    }
    #region test dummies
    [Controller("Test5QueueName")]
    private class Test5Controller
    {
        [Execute("TriggeredFromOtherCommand")]
        public string Handle(Test5Command command)
        {
            return "result";
        }
    }
    public class Test5Command
    {
    }
    #endregion

    [Fact]
    public void AddControllerAllInOne()
    {
        var target = new MicroserviceHostBuilder();

        target.AddController<Test6Controller>();

        var controller = target.Controllers.First();
        Assert.Equal(5, controller.Commands.Count());
        Assert.Contains(controller.Commands, ch => ch.Key == typeof(Test2Command).ToString());
        Assert.Contains(controller.Commands, ch => ch.Key == typeof(Test3Command).ToString());
        Assert.Contains(controller.Commands, ch => ch.Key == typeof(Test4Command).ToString());
        Assert.Contains(controller.Commands, ch => ch.Key == "TriggeredFromOtherCommand");
        Assert.Contains(controller.Commands, ch => ch.Key == "TriggeredFromYetAnotherCommand");
    }
    #region test dummies
    [Controller("Test6QueueName")]
    private class Test6Controller
    {
        public string Execute(Test2Command command)
        {
            return "result";
        }
        [Execute]
        public string Execute(Test3Command command)
        {
            return "result";
        }
        [Execute]
        public string Handle(Test4Command command)
        {
            return "result";
        }
        [Execute("TriggeredFromOtherCommand")]
        public string Execute(Test4Command command)
        {
            return "result";
        }
        [Execute("TriggeredFromYetAnotherCommand")]
        public string Handle(Test3Command command)
        {
            return "result";
        }
    }
    #endregion

    [Fact]
    public void CreatHostTransfersControllers()
    {
        var target = new MicroserviceHostBuilder()
            .AddController<Test3Controller>()
            .AddController<Test5Controller>()
            .AddController<Test6Controller>();

        var host = target.CreateHost();

        Assert.Equal(3, host.Controllers.Count());
    }
}
