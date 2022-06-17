using System.Text;
using Cronical;
using Cronical.Configuration;
using Cronical.Jobs;
using FluentAssertions;
using Xunit;

namespace CronicalTests.Misc;

public class SendEmailEncodingsTest : IDisposable
{
    private readonly CronJob _job;
    private readonly string _path;
    private readonly MockMailSender _sender;

    public SendEmailEncodingsTest()
    {
        _path = Path.GetTempFileName();

        _sender = new MockMailSender();
        Program.MailSender = _sender;

        _job = new CronJob
        {
            Command = "cmd /c type " + _path,
            Settings = new JobSettings
            {
                MailTo = "test@localhost",
                MailFrom = "cronical@localhost",
                MailStdOut = true,
                SmtpHost = "localhost"
            }
        };
    }

    public void Dispose()
    {
        File.Delete(_path);
    }

    [Fact]
    public void TestSendWithCp1252()
    {
        File.WriteAllBytes(_path, Encoding.Default.GetBytes("abc åäö ÅÄÖ"));
        _job.RunJobThread();

        var email = _sender.SentEmails.Single();
        email.Message.Body.Contains("abc åäö ÅÄÖ").Should().BeTrue();
    }

    [Fact]
    public void TestSendWithUtf8()
    {
        File.WriteAllBytes(_path, Encoding.UTF8.GetBytes("abc åäö ÅÄÖ"));
        _job.RunJobThread();

        var email = _sender.SentEmails.Single();
        email.Message.Body.Contains("abc åäö ÅÄÖ").Should().BeTrue();
    }
}