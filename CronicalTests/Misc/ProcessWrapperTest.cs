using Cronical.Misc;
using FluentAssertions;
using Xunit;

namespace CronicalTests.Misc;

public class ProcessWrapperTest
{
    private readonly string _path;

    public ProcessWrapperTest()
    {
        _path = Directory.GetCurrentDirectory();
    }

    [Fact]
    public void TestStart_DefaultPath()
    {
        var wrapper = new ProcessWrapper("cmd /c cd", _path, false, true);
        wrapper.Start();
        wrapper.WaitForEnd(5000);

        var result = wrapper.FetchResult().Trim();

        result.ToLower().Should().Be(Directory.GetCurrentDirectory().ToLower());
    }

    [Fact]
    public void TestStart_GivenPath()
    {
        var wrapper = new ProcessWrapper("c:\\windows\\system32\\cmd /c cd", _path, false, true);
        wrapper.Start();
        wrapper.WaitForEnd(5000);

        var result = wrapper.FetchResult().Trim();

        result.ToLower().Should().Be("c:\\windows\\system32".ToLower());
    }

    [Fact]
    public void TestStart_RelativePath()
    {
        var wrapper = new ProcessWrapper("system32\\cmd /c cd", "c:\\windows", false, true);
        wrapper.Start();
        wrapper.WaitForEnd(5000);

        var result = wrapper.FetchResult().Trim();

        result.ToLower().Should().Be("c:\\windows\\system32".ToLower());
    }
}