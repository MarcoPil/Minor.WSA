using Minor.WSA.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;


public class BusOptionsTest
{
    [Fact]
    public void DefaultBusoptions()
    {
        // Act
        var result = new BusOptions();

        // Assert
        Assert.Equal("WSA.DefaultEventBus", result.ExchangeName);
        Assert.Equal("localhost", result.HostName);
        Assert.Equal(5672, result.Port);
        Assert.Equal("guest", result.UserName);
        Assert.Equal("guest", result.Password);
    }

    [Fact]
    public void PartiallyDefaultBusoptions()
    {
        // Act
        var result = new BusOptions(exchangeName: "Alternative.Eventbus",
                                    password: "Gast!");

        // Assert
        Assert.Equal("Alternative.Eventbus", result.ExchangeName);
        Assert.Equal("localhost", result.HostName);
        Assert.Equal(5672, result.Port);
        Assert.Equal("guest", result.UserName);
        Assert.Equal("Gast!", result.Password);
    }

    [Fact]
    public void CopyBusoptions()
    {
        // Arrange
        var target = new BusOptions();

        // Act
        BusOptions result = target.CopyWith(
                        exchangeName: "Eventbus",
                        hostName: "ServerName",
                        port: 12345,
                        userName: "Jan",
                        password: "&Alleman");

        // Assert
        Assert.Equal("WSA.DefaultEventBus", target.ExchangeName);
        Assert.Equal("localhost", target.HostName);
        Assert.Equal(5672, target.Port);
        Assert.Equal("guest", target.UserName);
        Assert.Equal("guest", target.Password);

        Assert.Equal("Eventbus", result.ExchangeName);
        Assert.Equal("ServerName", result.HostName);
        Assert.Equal(12345, result.Port);
        Assert.Equal("Jan", result.UserName);
        Assert.Equal("&Alleman", result.Password);
    }

    [Fact]
    public void PartialCopyBusoptions()
    {
        // Arrange
        var target = new BusOptions();

        // Act
        BusOptions result = target.CopyWith(
                        exchangeName: "Eventbus",
                        password: "&Alleman");

        // Assert
        Assert.Equal("Eventbus", result.ExchangeName);
        Assert.Equal("localhost", result.HostName);
        Assert.Equal(5672, result.Port);
        Assert.Equal("guest", result.UserName);
        Assert.Equal("&Alleman", result.Password);
    }

    [Fact]
    public void BusoptionsFromEnvironment()
    {
        // Arrange
        string backup1 = Environment.GetEnvironmentVariable("eventbus-exchangename");
        string backup2 = Environment.GetEnvironmentVariable("eventbus-hostname");
        string backup3 = Environment.GetEnvironmentVariable("eventbus-port");
        string backup4 = Environment.GetEnvironmentVariable("eventbus-username");
        string backup5 = Environment.GetEnvironmentVariable("eventbus-password");
        Environment.SetEnvironmentVariable("eventbus-exchangename", "envExchangeName");
        Environment.SetEnvironmentVariable("eventbus-hostname", "envHostName");
        Environment.SetEnvironmentVariable("eventbus-port", "51422");
        Environment.SetEnvironmentVariable("eventbus-username", "envUserName");
        Environment.SetEnvironmentVariable("eventbus-password", "envPassword");

        // Act
        var result = BusOptions.CreateFromEnvironment();

        // Assert
        Assert.Equal("envExchangeName", result.ExchangeName);
        Assert.Equal("envHostName", result.HostName);
        Assert.Equal(51422, result.Port);
        Assert.Equal("envUserName", result.UserName);
        Assert.Equal("envPassword", result.Password);

        // Clean-up
        Environment.SetEnvironmentVariable("eventbus-exchangename", backup1);
        Environment.SetEnvironmentVariable("eventbus-hostname", backup2);
        Environment.SetEnvironmentVariable("eventbus-port", backup3);
        Environment.SetEnvironmentVariable("eventbus-username", backup4);
        Environment.SetEnvironmentVariable("eventbus-password", backup5);
    }

    [Fact]
    public void BusOptionsToString()
    {
        // Arrange
        var target = new BusOptions
        (
            exchangeName: "Eventbus",
            hostName: "ServerName",
            port: 12345,
            userName: "Jan",
            password: "&Alleman"
        );

        // Act
        var result = target.ToString();

        //
        // Assert
        Assert.Equal("{\r\n  \"ExchangeName\": \"Eventbus\",\r\n  \"HostName\": \"ServerName\",\r\n  \"Port\": 12345,\r\n  \"UserName\": \"Jan\",\r\n  \"Password\": \"&Alleman\"\r\n}", result.ToString());
    }
}