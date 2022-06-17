using Cronical;
using Cronical.Configuration;
using Cronical.Jobs;
using FluentAssertions;
using Xunit;

namespace CronicalTests.Misc;

public class SendEmailTest
{
    private readonly MockMailSender _sender;
    private readonly CronJob _job;

    public SendEmailTest()
    {
        _sender = new MockMailSender();
        Program.MailSender = _sender;

        _job = new CronJob
        {
            Command = "cmd /c help",
            Settings = new JobSettings
            {
                MailTo = "test@localhost",
                MailFrom = "cronical@localhost",
                MailStdOut = true,
                SmtpHost = "localhost"
            }
        };
    }

    [Fact]
    public void TestSendEmail()
    {
        _job.RunJobThread();

        var email = _sender.SentEmails.Single();
        email.Host.Should().Be("localhost");
        email.Credentials.Should().BeNull();
        email.Ssl.Should().BeFalse();
        email.Message.From.Address.Should().Be("cronical@localhost");
        email.Message.To.Single().Address.Should().Be("test@localhost");
        email.Message.CC.Any().Should().BeFalse();
        email.Message.Bcc.Any().Should().BeFalse();
        email.Message.Body.Contains("XCOPY").Should().BeTrue();
    }

    [Fact]
    public void TestSendEmailNoStdCapture()
    {
        _job.Settings.MailStdOut = false;
        _job.RunJobThread();

        Console.WriteLine(string.Join("\r\n", _sender.SentEmails.Select(x => x.Message.Body)));
        _sender.SentEmails.Any().Should().BeFalse();
    }

    [Fact]
    public void TestSendEmailCc()
    {
        _job.Settings.MailCc = "president@whitehouse";
        _job.Settings.MailBcc = "vice-president@whitehouse";
        _job.RunJobThread();

        var email = _sender.SentEmails.Single();
        email.Host.Should().Be("localhost");
        email.Credentials.Should().BeNull();
        email.Ssl.Should().BeFalse();
        email.Message.From.Address.Should().Be("cronical@localhost");
        email.Message.To.Single().Address.Should().Be("test@localhost");
        email.Message.CC.Single().Address.Should().Be("president@whitehouse");
        email.Message.Bcc.Single().Address.Should().Be("vice-president@whitehouse");
        email.Message.Body.Contains("XCOPY").Should().BeTrue();
    }

    [Fact]
    public void TestSendEmailCredAndSsl()
    {
        _job.Settings.SmtpSSL = true;
        _job.Settings.SmtpUser = "root";
        _job.Settings.SmtpPass = "secret";
        _job.RunJobThread();

        var email = _sender.SentEmails.Single();
        email.Host.Should().Be("localhost");
        email.Credentials.UserName.Should().Be("root");
        email.Credentials.Password.Should().Be("secret");
        email.Ssl.Should().BeTrue();
        email.Message.From.Address.Should().Be("cronical@localhost");
        email.Message.To.Single().Address.Should().Be("test@localhost");
        email.Message.CC.Any().Should().BeFalse();
        email.Message.Bcc.Any().Should().BeFalse();
        email.Message.Body.Contains("XCOPY").Should().BeTrue();
    }
}