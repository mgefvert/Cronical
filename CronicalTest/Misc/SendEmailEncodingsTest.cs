using System;
using System.IO;
using System.Linq;
using System.Text;
using Cronical.Configuration;
using Cronical.Jobs;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cronical.Test.Misc
{
    [TestClass]
    public class SendEmailEncodingsTest
    {
        private CronJob _job;
        private string _path;
        private MockMailSender _sender;

        [TestInitialize]
        public void Setup()
        {
            _path = Path.GetTempFileName();

            _sender = new MockMailSender();
            Program.MailSender = _sender;

            _job = new CronJob
            {
                Command = "cmd /c type " + _path,
                Settings = new Settings
                {
                    MailTo = "test@localhost",
                    MailFrom = "cronical@localhost",
                    MailStdOut = true,
                    SmtpHost = "localhost"
                }
            };
        }

        [TestCleanup]
        public void Teardown()
        {
            File.Delete(_path);
        }

        [TestMethod]
        public void TestSendWithCp1252()
        {
            File.WriteAllBytes(_path, Encoding.Default.GetBytes("abc åäö ÅÄÖ"));
            _job.RunJobThread();

            var email = _sender.SentEmails.Single();
            Assert.IsTrue(email.Message.Body.Contains("abc åäö ÅÄÖ"));
        }

        [TestMethod]
        public void TestSendWithUtf8()
        {
            File.WriteAllBytes(_path, Encoding.UTF8.GetBytes("abc åäö ÅÄÖ"));
            _job.RunJobThread();

            var email = _sender.SentEmails.Single();
            Assert.IsTrue(email.Message.Body.Contains("abc åäö ÅÄÖ"));
        }
    }
}
