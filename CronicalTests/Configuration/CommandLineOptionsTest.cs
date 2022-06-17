using Cronical.Configuration;
using DotNetCommons.Sys;
using FluentAssertions;
using Xunit;

namespace CronicalTests.Configuration;

public class CommandLineOptionsTest
{
    public CommandLineOptionsTest()
    {
        CommandLine.DisplayHelpOnEmpty = false;
    }

    [Fact]
    public void TestNone()
    {
        var cmd = CommandLine.Parse<CommandLineOptions>(new string[0]);

        cmd.ConfigFile.Should().Be("cronical.dat");
        cmd.ConfigFileOverride.Should().BeFalse();
        cmd.DebugLogs.Should().BeFalse();
        cmd.Help.Should().BeFalse();
        cmd.InstallService.Should().BeFalse();
        cmd.RemoveService.Should().BeFalse();
        cmd.RunAsConsole.Should().BeFalse();
        cmd.ServiceName.Should().Be("Cronical");
        cmd.ServiceTitle.Should().Be("Cronical Job Scheduler");
        cmd.ServiceDescription.Should().BeNull();
    }

    [Fact]
    public void TestMany()
    {
        var cmd = CommandLine.Parse<CommandLineOptions>("-d", "--install", "--remove", "--console", "-c", "test.dat", "-h",
            "--service-name=c1", "--service-title=\"Cronical 1\"", "--service-desc=\"Cronical instance 1\"");

        cmd.ConfigFile.Should().Be("test.dat");
        cmd.ConfigFileOverride.Should().BeTrue();
        cmd.DebugLogs.Should().BeTrue();
        cmd.Help.Should().BeTrue();
        cmd.InstallService.Should().BeTrue();
        cmd.RemoveService.Should().BeTrue();
        cmd.RunAsConsole.Should().BeTrue();
        cmd.ServiceName.Should().Be("c1");
        cmd.ServiceTitle.Should().Be("Cronical 1");
        cmd.ServiceDescription.Should().Be("Cronical instance 1");
    }

    [Fact]
    public void TestHelp()
    {
        CommandLine.Parse<CommandLineOptions>("-h").Help.Should().BeTrue();
        CommandLine.Parse<CommandLineOptions>("--help").Help.Should().BeTrue();
        CommandLine.Parse<CommandLineOptions>("-?").Help.Should().BeTrue();
        CommandLine.Parse<CommandLineOptions>("/h").Help.Should().BeTrue();
    }

    [Fact]
    public void TestOverride()
    {
        var cmd = CommandLine.Parse<CommandLineOptions>("--config=test.dat");
        cmd.ConfigFile.Should().Be("test.dat");
        cmd.ConfigFileOverride.Should().BeTrue();

        cmd = CommandLine.Parse<CommandLineOptions>("--config", "test.dat");
        cmd.ConfigFile.Should().Be("test.dat");
        cmd.ConfigFileOverride.Should().BeTrue();

        cmd = CommandLine.Parse<CommandLineOptions>("-c", "test.dat");
        cmd.ConfigFile.Should().Be("test.dat");
        cmd.ConfigFileOverride.Should().BeTrue();
    }
}