using Minor.WSA.Common;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

public class ErrorTests
{
    [Fact]
    public void ErrorToString()
    {
        var target = new Error("MyCode", "MyMessage");
        var result = target.ToString();
        Assert.Equal("Error(code:\"MyCode\", message:\"MyMessage\")", result);
    }

    [Fact]
    public void ErrorEquality()
    {
        var e1 = new Error("MyCode", "MyMessage");
        var e2 = new Error("MyCode", "MyMessage");
        Assert.True(e1 == e2);
        Assert.True(e1.Equals(e2));
        Assert.False(e1 != e2);
        Assert.Equal(e1.GetHashCode(), e2.GetHashCode());
    }

    [Fact]
    public void ErrorInequality()
    {
        var e1 = new Error("MyCode", "MyMessage");
        var e2 = new Error("OtherCode", "MyMessage");
        var e3 = new Error("MyCode", "OtherMessage");
        Assert.False(e1 == e2);
        Assert.False(e1.Equals(e2));
        Assert.True(e1 != e2);
        Assert.False(e1 == e3);
    }
}

