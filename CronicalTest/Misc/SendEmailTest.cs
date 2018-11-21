using System;
using System.Linq;
using Cronical.Configuration;
using Cronical.Jobs;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cronical.Test.Misc
{
    [TestClass]
    public class SendEmailTest
    {
        private MockMailSender _sender;
        private CronJob _job;

        [TestInitialize]
        public void Setup()
        {
            _sender = new MockMailSender();
            Program.MailSender = _sender;

            _job = new CronJob
            {
                Command = "cmd /c help",
                Settings = new Settings
                {
                    MailTo = "test@localhost",
                    MailFrom = "cronical@localhost",
                    MailStdOut = true,
                    SmtpHost = "localhost"
                }
            };
        }

        [TestMethod]
        public void TestSendEmail()
        {
            _job.RunJobThread();

            var email = _sender.SentEmails.Single();
            Assert.AreEqual("localhost", email.Host);
            Assert.AreEqual(null, email.Credentials);
            Assert.AreEqual(false, email.Ssl);
            Assert.AreEqual("cronical@localhost", email.Message.From.Address);
            Assert.AreEqual("test@localhost", email.Message.To.Single().Address);
            Assert.IsFalse(email.Message.CC.Any());
            Assert.IsFalse(email.Message.Bcc.Any());
            Assert.IsTrue(email.Message.Body.Contains("XCOPY"));
        }

        [TestMethod]
        public void TestSendEmailNoStdCapture()
        {
            _job.Settings.MailStdOut = false;
            _job.RunJobThread();

            Console.WriteLine(string.Join("\r\n", _sender.SentEmails.Select(x => x.Message.Body)));
            Assert.IsFalse(_sender.SentEmails.Any());
        }

        [TestMethod]
        public void TestSendEmailCc()
        {
            _job.Settings.MailCc = "president@whitehouse";
            _job.Settings.MailBcc = "vice-president@whitehouse";
            _job.RunJobThread();

            var email = _sender.SentEmails.Single();
            Assert.AreEqual("localhost", email.Host);
            Assert.AreEqual(null, email.Credentials);
            Assert.AreEqual(false, email.Ssl);
            Assert.AreEqual("cronical@localhost", email.Message.From.Address);
            Assert.AreEqual("test@localhost", email.Message.To.Single().Address);
            Assert.AreEqual("president@whitehouse", email.Message.CC.Single().Address);
            Assert.AreEqual("vice-president@whitehouse", email.Message.Bcc.Single().Address);
            Assert.IsTrue(email.Message.Body.Contains("XCOPY"));
        }

        [TestMethod]
        public void TestSendEmailCredAndSsl()
        {
            _job.Settings.SmtpSSL = true;
            _job.Settings.SmtpUser = "root";
            _job.Settings.SmtpPass = "secret";
            _job.RunJobThread();

            var email = _sender.SentEmails.Single();
            Assert.AreEqual("localhost", email.Host);
            Assert.AreEqual("root", email.Credentials.UserName);
            Assert.AreEqual("secret", email.Credentials.Password);
            Assert.AreEqual(true, email.Ssl);
            Assert.AreEqual("cronical@localhost", email.Message.From.Address);
            Assert.AreEqual("test@localhost", email.Message.To.Single().Address);
            Assert.IsFalse(email.Message.CC.Any());
            Assert.IsFalse(email.Message.Bcc.Any());
            Assert.IsTrue(email.Message.Body.Contains("XCOPY"));
        }
    }
}
