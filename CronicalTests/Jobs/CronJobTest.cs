using Cronical.Configuration;
using Cronical.Integrations;
using FluentAssertions;
using Xunit;

// ReSharper disable LocalizableElement

namespace CronicalTests.Jobs;

public class CronJobTest
{
    [Fact]
    public void TestGetCode()
    {
        var job = FileConfigReader.ParseJob("0,15,30,45 * * */2 * process.exe", new JobSettings
        {
            Home = "c:\\windows",
            Timeout = 86400
        });

        job.GetJobCode().Should().Be("CronJob,process.exe,c:\\windows,False,,,,,,,False,,86400,,127,1365,2147483647,16777215,35185445863425");
    }
}