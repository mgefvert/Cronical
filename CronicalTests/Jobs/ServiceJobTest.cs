using Cronical.Configuration;
using Cronical.Jobs;
using FluentAssertions;
using Xunit;

namespace CronicalTests.Jobs;

public class ServiceJobTest
{
    [Fact]
    public void TestGetCode()
    {
        var job = new ServiceJob
        {
            Command = "process.exe",
            Settings = new JobSettings
            {
                Home = "c:\\windows",
                Timeout = 86400
            }
        };

        job.GetJobCode().Should().Be("ServiceJob,process.exe,c:\\windows,False,,,,,,,False,,86400");
    }
}