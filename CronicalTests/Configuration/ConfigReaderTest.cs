using System.Reflection;
using Cronical.Configuration;
using Cronical.Integrations;
using Cronical.JobRunners;
using Cronical.Misc;
using FluentAssertions;
using Xunit;

namespace CronicalTests.Configuration;

public class EnvironmentTest
{
    [Fact]
    public void TestStripComments()
    {
        FileConfigReader.PreprocessLine(null).Should().BeNull();
        FileConfigReader.PreprocessLine("").Should().BeNull();
        FileConfigReader.PreprocessLine("       ").Should().BeNull();
        FileConfigReader.PreprocessLine("   # This is some whitespace").Should().BeNull();
    }

    [Fact]
    public void TestSpacing()
    {
        var job = FileConfigReader.ParseJob(" * * * * *    xx\t1\t2");
        job.Command.Should().Be("xx 1 2");
    }

    [Fact]
    public void TestTrimComment()
    {
        FileConfigReader.PreprocessLine(null).Should().BeNull();
        FileConfigReader.PreprocessLine("").Should().BeNull();
        FileConfigReader.PreprocessLine("   ").Should().BeNull();
        FileConfigReader.PreprocessLine(" Hello ").Should().Be("Hello");
        FileConfigReader.PreprocessLine("#").Should().BeNull();
        FileConfigReader.PreprocessLine("##").Should().BeNull();
        FileConfigReader.PreprocessLine("Text # Comment").Should().Be("Text");
        FileConfigReader.PreprocessLine("Text # Comment # Again").Should().Be("Text");
        FileConfigReader.PreprocessLine("Text \\# More Text # Comment").Should().Be("Text # More Text");
        FileConfigReader.PreprocessLine("Text # Comment \\").Should().Be("Text");
    }

    [Fact]
    public void TestLoad()
    {
        var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Cronical.Test.cronical.dat");
        stream.Should().NotBeNull();

        var globalSettings = new GlobalSettings();
        var jobSettings = new JobSettings { Home = "c:\\test" };

        var jobs = FileConfigReader.LoadConfig(stream, globalSettings, jobSettings).ToList();

        globalSettings.RunMissedJobs.Should().BeTrue();
        globalSettings.ServiceChecks.Should().Be(15);

        jobs.Count.Should().Be(17);

        var cronJobs = jobs.OfType<CronJob>().ToArray();
        cronJobs.Length.Should().Be(15);

        // @reboot cmd /c echo Hello, world!
        var cronJob = cronJobs[0];
        cronJob.Command.Should().Be("cmd /c echo Hello, world!");
        cronJob.Reboot.Should().BeTrue();
        cronJob.Minutes.Val().Should().Be(0ul);
        cronJob.Hours.Val().Should().Be(0ul);
        cronJob.Days.Val().Should().Be(0ul);
        cronJob.Months.Val().Should().Be(0ul);
        cronJob.Weekdays.Val().Should().Be(0ul);

        cronJob.Settings.Home.Should().Be("c:\\test");
        cronJob.Settings.Timeout.Should().Be(3600);
        cronJob.Settings.MailFrom.Should().Be("cronical@example.com");
        cronJob.Settings.MailTo.Should().Be("admin@example.com");
        cronJob.Settings.MailStdOut.Should().BeFalse();
        cronJob.Settings.MailCc.Should().Be("cc@example.com");
        cronJob.Settings.MailBcc.Should().Be("bcc@example.com");
        cronJob.Settings.SmtpHost.Should().Be("mail.example.com");
        cronJob.Settings.SmtpSSL.Should().BeFalse();
        cronJob.Settings.SmtpUser.Should().Be("root@example.com");
        cronJob.Settings.SmtpPass.Should().Be("password");

        // * * * * *  cmd /c echo Every minute
        cronJob = cronJobs[1];
        cronJob.Command.Should().Be("cmd /c echo Every minute");
        cronJob.Reboot.Should().BeFalse();
        cronJob.Minutes.Val().Should().Be(MakeAllBits(60));
        cronJob.Hours.Val().Should().Be(MakeAllBits(24));
        cronJob.Days.Val().Should().Be(MakeAllBits(31));
        cronJob.Months.Val().Should().Be(MakeAllBits(12));
        cronJob.Weekdays.Val().Should().Be(MakeAllBits(7));

        // */5 * * * * cmd /c echo Every five minutes
        cronJob = cronJobs[2];
        cronJob.Command.Should().Be("cmd /c echo Every five minutes");
        cronJob.Reboot.Should().BeFalse();
        cronJob.Minutes.Val().Should().Be(MakeBits(0, 5, 10, 15, 20, 25, 30, 35, 40, 45, 50, 55));
        cronJob.Hours.Val().Should().Be(MakeAllBits(24));
        cronJob.Days.Val().Should().Be(MakeAllBits(31));
        cronJob.Months.Val().Should().Be(MakeAllBits(12));
        cronJob.Weekdays.Val().Should().Be(MakeAllBits(7));

        // 0 8,17 * * * cmd /c echo At 08:00, 12:00 and 17:00
        cronJob = cronJobs[3];
        cronJob.Command.Should().Be("cmd /c echo At 08:00, 12:00 and 17:00");
        cronJob.Minutes.Val().Should().Be(MakeBits(0));
        cronJob.Hours.Val().Should().Be(MakeBits(8, 17));

        // 0,30 * * * * cmd /c echo Every hour at :00, :15, :30 and :45
        cronJob = cronJobs[4];
        cronJob.Command.Should().Be("cmd /c echo Every hour at :00, :15, :30 and :45");
        cronJob.Minutes.Val().Should().Be(MakeBits(0, 30));

        // 0 2 1 * * cmd /c echo At 02:00 the 1st of every month
        cronJob = cronJobs[5];
        cronJob.Command.Should().Be("cmd /c echo At 02:00 the 1st of every month");
        cronJob.Minutes.Val().Should().Be(MakeBits(0));
        cronJob.Hours.Val().Should().Be(MakeBits(2));
        cronJob.Days.Val().Should().Be(MakeBits(0));

        // 0 2 1 1 * cmd /c echo At 02:00 the 1st of March
        cronJob = cronJobs[6];
        cronJob.Command.Should().Be("cmd /c echo At 02:00 the 1st of March");
        cronJob.Minutes.Val().Should().Be(MakeBits(0));
        cronJob.Hours.Val().Should().Be(MakeBits(2));
        cronJob.Days.Val().Should().Be(MakeBits(0));
        cronJob.Months.Val().Should().Be(MakeBits(2));

        // 30 2 * * sat,sun cmd /c echo At 02:30 every Saturday or Sunday
        cronJob = cronJobs[7];
        cronJob.Command.Should().Be("cmd /c echo At 02:30 every Saturday or Sunday");
        cronJob.Minutes.Val().Should().Be(MakeBits(30));
        cronJob.Hours.Val().Should().Be(MakeBits(2));
        cronJob.Weekdays.Val().Should().Be(MakeBits(0, 6));

        // @reboot cmd /c echo Run when the service is started (typically on reboot)
        cronJob = cronJobs[8];
        cronJob.Command.Should().Be("cmd /c echo Run when the service is started (typically on reboot)");
        cronJob.Reboot.Should().BeTrue();

        cronJob.Settings.Home.Should().Be("c:\\examples");
        cronJob.Settings.Timeout.Should().Be(180);
        cronJob.Settings.MailFrom.Should().Be("cronical@example.com");
        cronJob.Settings.MailTo.Should().Be("admin@example.com");
        cronJob.Settings.MailStdOut.Should().BeTrue();
        cronJob.Settings.MailCc.Should().Be("cc@example.com");
        cronJob.Settings.MailBcc.Should().Be("bcc@example.com");
        cronJob.Settings.SmtpHost.Should().Be("mail.example.com");
        cronJob.Settings.SmtpSSL.Should().BeFalse();
        cronJob.Settings.SmtpUser.Should().Be("root@example.com");
        cronJob.Settings.SmtpPass.Should().Be("password");

        // @yearly cmd /c echo Once a year, typically midnight Jan 1st
        cronJob = cronJobs[9];
        cronJob.Command.Should().Be("cmd /c echo Once a year, typically midnight Jan 1st");
        cronJob.Minutes.Val().Should().Be(MakeBits(0));
        cronJob.Hours.Val().Should().Be(MakeBits(0));
        cronJob.Days.Val().Should().Be(MakeBits(0));
        cronJob.Months.Val().Should().Be(MakeBits(0));
        cronJob.Weekdays.Val().Should().Be(MakeBits(7));

        // @annually cmd /c echo Same as @yearly
        cronJob = cronJobs[10];
        cronJob.Command.Should().Be("cmd /c echo Same as @yearly");
        cronJob.Minutes.Val().Should().Be(MakeBits(0));
        cronJob.Hours.Val().Should().Be(MakeBits(0));
        cronJob.Days.Val().Should().Be(MakeBits(0));
        cronJob.Months.Val().Should().Be(MakeBits(0));
        cronJob.Weekdays.Val().Should().Be(MakeBits(7));

        // @monthly cmd /c echo Midnight on the first of every month
        cronJob = cronJobs[11];
        cronJob.Command.Should().Be("cmd /c echo Midnight on the first of every month");
        cronJob.Minutes.Val().Should().Be(MakeAllBits(0));
        cronJob.Hours.Val().Should().Be(MakeAllBits(0));
        cronJob.Days.Val().Should().Be(MakeAllBits(0));
        cronJob.Months.Val().Should().Be(MakeAllBits(12));
        cronJob.Weekdays.Val().Should().Be(MakeAllBits(7));

        // @weekly cmd /c echo Midnight on every Sunday
        cronJob = cronJobs[12];
        cronJob.Command.Should().Be("cmd /c echo Midnight on every Sunday");
        cronJob.Minutes.Val().Should().Be(MakeBits(0));
        cronJob.Hours.Val().Should().Be(MakeBits(0));
        cronJob.Days.Val().Should().Be(MakeBits(31));
        cronJob.Months.Val().Should().Be(MakeBits(12));
        cronJob.Weekdays.Val().Should().Be(MakeBits(0));

        // @daily cmd /c echo Midnight every day
        cronJob = cronJobs[13];
        cronJob.Command.Should().Be("cmd /c echo Midnight every day");
        cronJob.Minutes.Val().Should().Be(MakeAllBits(0));
        cronJob.Hours.Val().Should().Be(MakeAllBits(0));
        cronJob.Days.Val().Should().Be(MakeAllBits(31));
        cronJob.Months.Val().Should().Be(MakeAllBits(12));
        cronJob.Weekdays.Val().Should().Be(MakeAllBits(7));

        // @hourly cmd /c echo Every hour
        cronJob = cronJobs[14];
        cronJob.Command.Should().Be("cmd /c echo Every hour");
        cronJob.Minutes.Val().Should().Be(MakeAllBits(0));
        cronJob.Hours.Val().Should().Be(MakeAllBits(24));
        cronJob.Days.Val().Should().Be(MakeAllBits(31));
        cronJob.Months.Val().Should().Be(MakeAllBits(12));
        cronJob.Weekdays.Val().Should().Be(MakeAllBits(7));

        var serviceJobs = jobs.OfType<ServiceJobRunner>().ToArray();
        serviceJobs.Length.Should().Be(2);
        serviceJobs[0].Command.Should().Be("cmd /k echo Start and keep running");
        serviceJobs[1].Command.Should().Be("cmd /k echo Start again and keep running");
    }

    [Fact]
    public void TestMakeBits()
    {
        MakeAllBits(6).Should().Be(0b00111111);
        MakeBits(1, 3).Should().Be(0b00001010);
    }

    private static ulong MakeBits(params int[] bits)
    {
        return bits.Aggregate<int, ulong>(0, (current, bit) => current | (ulong)1 << bit);
    }

    private static ulong MakeAllBits(int count)
    {
        ulong result = 0;

        for (int i = 0; i < count; i++)
            result |= (ulong)1 << i;

        return result;
    }
}