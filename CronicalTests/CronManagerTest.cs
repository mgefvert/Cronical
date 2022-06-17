using Cronical;
using Cronical.Configuration;
using Cronical.Integrations;
using FluentAssertions;
using Xunit;

namespace CronicalTests;

public class CronManagerTest
{
    private readonly CronManager _manager;

    public CronManagerTest()
    {
        _manager = new CronManager(new GlobalSettings(), new JobSettings(), new[] { new FileConfigReader("test.dat") });
    }

    private string ProcessWhitespace(string s)
    {
        s = s.Replace("\t", " ");
        while (s.Contains("  "))
            s = s.Replace("  ", " ");

        return s.Trim();
    }

    [Fact]
    [DeploymentItem("test.dat")]
    public void TestReadCronDat()
    {
        _manager.CronJobs.Count().Should().Be(8);
        _manager.ServiceJobs.Count().Should().Be(1);

        var job = _manager.CronJobs.SingleOrDefault(x => x.Command.Contains("This should be visible"));
        job.Should().NotBeNull();
        ProcessWhitespace(job.Command).Should().Be("cmd /c echo # This should be visible");
    }

    [Fact]
    [DeploymentItem("test.dat")]
    public void TestReloadCronDat()
    {
        _manager.CronJobs.Count().Should().Be(8);
        _manager.ServiceJobs.Count().Should().Be(1);

        // Reload with config changed = true
        _manager.Reload();

        _manager.CronJobs.Count().Should().Be(8);
        _manager.ServiceJobs.Count().Should().Be(1);
    }
}