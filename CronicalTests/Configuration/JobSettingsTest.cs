using Cronical.Configuration;
using FluentAssertions;
using Xunit;

namespace CronicalTests.Configuration;

public class JobSettingsTest
{
    [Fact]
    public void TestClone()
    {
        var env = new JobSettings
        {
            Home = "home",
            MailTo = "to-email",
            MailFrom = "from-email",
            SmtpHost = "server",
            SmtpPass = "password",
            SmtpUser = "user"
        };

        var env2 = env.Clone();

        env.Should().NotBeSameAs(env2);
        env.Home.Should().Be(env2.Home);
        env.MailFrom.Should().Be(env2.MailFrom);
        env.MailTo.Should().Be(env2.MailTo);
        env.SmtpHost.Should().Be(env2.SmtpHost);
        env.SmtpPass.Should().Be(env2.SmtpPass);
        env.SmtpUser.Should().Be(env2.SmtpUser);
    }

    [Fact]
    public void TestExists()
    {
        var env = new JobSettings();

        env.Exists("home").Should().BeTrue();
        env.Exists("MAILTO").Should().BeTrue();
        env.Exists("SmtpUser").Should().BeTrue();
        env.Exists("NotExists").Should().BeFalse();
    }

    [Fact]
    public void TestSet()
    {
        var env = new JobSettings();

        env.Set("home", "bork").Should().BeTrue();
        env.Set("MAILTO", "xxx").Should().BeTrue();

        env.Home.Should().Be("bork");
        env.MailTo.Should().Be("xxx");
    }

    [Fact]
    public void TestSetInvalid()
    {
        var env = new JobSettings();
        env.Set("xxx", "bork").Should().BeFalse();
    }
}