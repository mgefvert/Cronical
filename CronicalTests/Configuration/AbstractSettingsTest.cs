using Cronical.Configuration;
using FluentAssertions;
using Xunit;

namespace CronicalTests.Configuration;

public class SampleTest : AbstractSettings
{
    public string S { get; set; }
    public int N { get; set; }
    public bool B { get; set; }
}

public class AbstractSettingsTest
{
    private readonly SampleTest _x;

    public AbstractSettingsTest()
    {
        _x = new SampleTest();
    }

    [Fact]
    public void TestExists()
    {
        _x.Exists("s").Should().BeTrue();
        _x.Exists("n").Should().BeTrue();
        _x.Exists("b").Should().BeTrue();
        _x.Exists("x").Should().BeFalse();
        _x.Exists("S").Should().BeTrue();
        _x.Exists("N").Should().BeTrue();
        _x.Exists("B").Should().BeTrue();
        _x.Exists("X").Should().BeFalse();
        _x.Exists("").Should().BeFalse();
        _x.Exists(null).Should().BeFalse();
    }

    [Fact]
    public void TestSet()
    {
        _x.Set("s", "hello").Should().BeTrue();
        _x.Set("n", "42").Should().BeTrue();
        _x.Set("b", "true").Should().BeTrue();
        _x.Set("x", "bork").Should().BeFalse();

        _x.S.Should().Be("hello");
        _x.N.Should().Be(42);
        _x.B.Should().BeTrue();
    }

    [Fact]
    public void TestToString()
    {
        _x.S = "hello";
        _x.N = 42;

        _x.ToString().Should().Be("hello,42,False");
    }
}