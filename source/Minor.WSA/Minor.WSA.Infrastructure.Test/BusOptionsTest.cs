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
    public void BusOptionsToString()
    {
        // Arrange
        var target = new BusOptions
        {
            ExchangeName = "Eventbus",
            HostName = "ServerName",
            Port = 12345,
            UserName = "Jan",
            Password = "&Alleman",
        };

        // Act
        var result = target.ToString();

        //
        // Assert
        Assert.Equal("{\r\n  \"ExchangeName\": \"Eventbus\",\r\n  \"HostName\": \"ServerName\",\r\n  \"Port\": 12345,\r\n  \"UserName\": \"Jan\",\r\n  \"Password\": \"&Alleman\"\r\n}", result.ToString());
    }
}