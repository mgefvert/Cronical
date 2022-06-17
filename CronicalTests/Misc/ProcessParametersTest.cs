using Cronical.Misc;
using FluentAssertions;
using Xunit;

namespace CronicalTests.Misc;

public class ProcessParametersTest
{
    [Fact]
    public void Test()
    {
        var cmd = new ProcessParameters("mytest.exe one two three", "c:\\");
        cmd.Executable.Should().Be("c:\\mytest.exe");
        cmd.Directory.Should().Be("c:\\");
        cmd.Parameters.Should().Be("one two three");

        cmd = new ProcessParameters("mytest.exe", "c:\\windows\\system32");
        cmd.Executable.Should().Be("c:\\windows\\system32\\mytest.exe");
        cmd.Directory.Should().Be("c:\\windows\\system32");
        cmd.Parameters.Should().Be("");

        cmd = new ProcessParameters(".\\mytest.exe", "c:\\windows\\system32");
        cmd.Executable.Should().Be("c:\\windows\\system32\\mytest.exe");
        cmd.Directory.Should().Be("c:\\windows\\system32");
        cmd.Parameters.Should().Be("");

        cmd = new ProcessParameters("..\\mytest.exe", "c:\\windows\\system32");
        cmd.Executable.Should().Be("c:\\windows\\mytest.exe");
        cmd.Directory.Should().Be("c:\\windows");
        cmd.Parameters.Should().Be("");

        cmd = new ProcessParameters("..\\..\\mytest.exe", "c:\\windows\\system32");
        cmd.Executable.Should().Be("c:\\mytest.exe");
        cmd.Directory.Should().Be("c:\\");
        cmd.Parameters.Should().Be("");

        cmd = new ProcessParameters("c:\\mytest.exe", "c:\\windows\\system32");
        cmd.Executable.Should().Be("c:\\mytest.exe");
        cmd.Directory.Should().Be("c:\\");
        cmd.Parameters.Should().Be("");

        cmd = new ProcessParameters("c:\\local\\mytest.exe", "c:\\windows\\system32");
        cmd.Executable.Should().Be("c:\\local\\mytest.exe");
        cmd.Directory.Should().Be("c:\\local");
        cmd.Parameters.Should().Be("");
    }

    [Fact]
    public void TestCmd()
    {
        var cmd = new ProcessParameters("cmd /c dir", "c:\\");
        cmd.Executable.ToLower().Should().Be("c:\\windows\\system32\\cmd.exe");
        cmd.Directory.Should().Be("c:\\");
        cmd.Parameters.Should().Be("/c dir");
    }
}