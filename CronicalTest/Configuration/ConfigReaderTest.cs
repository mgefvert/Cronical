using System;
using System.Linq;
using System.Reflection;
using Cronical.Configuration;
using Cronical.Integrations;
using Cronical.Jobs;
using Cronical.Misc;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cronical.Test.Configuration
{
    [TestClass]
    public class EnvironmentTest
    {
        [TestMethod]
        public void TestStripComments()
        {
            Assert.IsNull(FileConfigReader.PreprocessLine(null));
            Assert.IsNull(FileConfigReader.PreprocessLine(""));
            Assert.IsNull(FileConfigReader.PreprocessLine("       "));
            Assert.IsNull(FileConfigReader.PreprocessLine("   # This is some whitespace"));
        }

        [TestMethod]
        public void TestSpacing()
        {
            var job = FileConfigReader.ParseJob(" * * * * *    xx\t1\t2");
            Assert.AreEqual("xx 1 2", job.Command);
        }

        [TestMethod]
        public void TestTrimComment()
        {
            Assert.AreEqual(null, FileConfigReader.PreprocessLine(null));
            Assert.AreEqual(null, FileConfigReader.PreprocessLine(""));
            Assert.AreEqual(null, FileConfigReader.PreprocessLine("   "));
            Assert.AreEqual("Hello", FileConfigReader.PreprocessLine(" Hello "));
            Assert.AreEqual(null, FileConfigReader.PreprocessLine("#"));
            Assert.AreEqual(null, FileConfigReader.PreprocessLine("##"));
            Assert.AreEqual("Text", FileConfigReader.PreprocessLine("Text # Comment"));
            Assert.AreEqual("Text", FileConfigReader.PreprocessLine("Text # Comment # Again"));
            Assert.AreEqual("Text # More Text", FileConfigReader.PreprocessLine("Text \\# More Text # Comment"));
            Assert.AreEqual("Text", FileConfigReader.PreprocessLine("Text # Comment \\"));
        }

        [TestMethod]
        public void TestLoad()
        {
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Cronical.Test.cronical.dat");
            Assert.IsNotNull(stream);

            var globalSettings = new GlobalSettings();
            var jobSettings = new JobSettings { Home = "c:\\test" };

            var jobs = FileConfigReader.LoadConfig(stream, globalSettings, jobSettings).ToList();

            Assert.IsTrue(globalSettings.RunMissedJobs);
            Assert.AreEqual(15, globalSettings.ServiceChecks);

            Assert.AreEqual(17, jobs.Count);
            
            var cronJobs = jobs.OfType<CronJob>().ToArray();
            Assert.AreEqual(15, cronJobs.Length);

            // @reboot cmd /c echo Hello, world!
            var cronJob = cronJobs[0];
            Assert.AreEqual("cmd /c echo Hello, world!", cronJob.Command);
            Assert.IsTrue(cronJob.Reboot);
            Assert.AreEqual(0ul, cronJob.Minutes.Val());
            Assert.AreEqual(0ul, cronJob.Hours.Val());
            Assert.AreEqual(0ul, cronJob.Days.Val());
            Assert.AreEqual(0ul, cronJob.Months.Val());
            Assert.AreEqual(0ul, cronJob.Weekdays.Val());

            Assert.AreEqual("c:\\test", cronJob.Settings.Home);
            Assert.AreEqual(3600, cronJob.Settings.Timeout);
            Assert.AreEqual("cronical@example.com", cronJob.Settings.MailFrom);
            Assert.AreEqual("admin@example.com", cronJob.Settings.MailTo);
            Assert.AreEqual(false, cronJob.Settings.MailStdOut);
            Assert.AreEqual("cc@example.com", cronJob.Settings.MailCc);
            Assert.AreEqual("bcc@example.com", cronJob.Settings.MailBcc);
            Assert.AreEqual("mail.example.com", cronJob.Settings.SmtpHost);
            Assert.AreEqual(false, cronJob.Settings.SmtpSSL);
            Assert.AreEqual("root@example.com", cronJob.Settings.SmtpUser);
            Assert.AreEqual("password", cronJob.Settings.SmtpPass);

            // * * * * *  cmd /c echo Every minute
            cronJob = cronJobs[1];
            Assert.AreEqual("cmd /c echo Every minute", cronJob.Command);
            Assert.IsFalse(cronJob.Reboot);
            Assert.AreEqual(MakeAllBits(60), cronJob.Minutes.Val());
            Assert.AreEqual(MakeAllBits(24), cronJob.Hours.Val());
            Assert.AreEqual(MakeAllBits(31), cronJob.Days.Val());
            Assert.AreEqual(MakeAllBits(12), cronJob.Months.Val());
            Assert.AreEqual(MakeAllBits(7), cronJob.Weekdays.Val());

            // */5 * * * * cmd /c echo Every five minutes
            cronJob = cronJobs[2];
            Assert.AreEqual("cmd /c echo Every five minutes", cronJob.Command);
            Assert.IsFalse(cronJob.Reboot);
            Assert.AreEqual(MakeBits(0, 5, 10, 15, 20, 25, 30, 35, 40, 45, 50, 55), cronJob.Minutes.Val());
            Assert.AreEqual(MakeAllBits(24), cronJob.Hours.Val());
            Assert.AreEqual(MakeAllBits(31), cronJob.Days.Val());
            Assert.AreEqual(MakeAllBits(12), cronJob.Months.Val());
            Assert.AreEqual(MakeAllBits(7), cronJob.Weekdays.Val());

            // 0 8,17 * * * cmd /c echo At 08:00, 12:00 and 17:00
            cronJob = cronJobs[3];
            Assert.AreEqual("cmd /c echo At 08:00, 12:00 and 17:00", cronJob.Command);
            Assert.AreEqual(MakeBits(0), cronJob.Minutes.Val());
            Assert.AreEqual(MakeBits(8, 17), cronJob.Hours.Val());

            // 0,30 * * * * cmd /c echo Every hour at :00, :15, :30 and :45
            cronJob = cronJobs[4];
            Assert.AreEqual("cmd /c echo Every hour at :00, :15, :30 and :45", cronJob.Command);
            Assert.AreEqual(MakeBits(0, 30), cronJob.Minutes.Val());

            // 0 2 1 * * cmd /c echo At 02:00 the 1st of every month
            cronJob = cronJobs[5];
            Assert.AreEqual("cmd /c echo At 02:00 the 1st of every month", cronJob.Command);
            Assert.AreEqual(MakeBits(0), cronJob.Minutes.Val());
            Assert.AreEqual(MakeBits(2), cronJob.Hours.Val());
            Assert.AreEqual(MakeBits(0), cronJob.Days.Val());

            // 0 2 1 1 * cmd /c echo At 02:00 the 1st of March
            cronJob = cronJobs[6];
            Assert.AreEqual("cmd /c echo At 02:00 the 1st of March", cronJob.Command);
            Assert.AreEqual(MakeBits(0), cronJob.Minutes.Val());
            Assert.AreEqual(MakeBits(2), cronJob.Hours.Val());
            Assert.AreEqual(MakeBits(0), cronJob.Days.Val());
            Assert.AreEqual(MakeBits(2), cronJob.Months.Val());

            // 30 2 * * sat,sun cmd /c echo At 02:30 every Saturday or Sunday
            cronJob = cronJobs[7];
            Assert.AreEqual("cmd /c echo At 02:30 every Saturday or Sunday", cronJob.Command);
            Assert.AreEqual(MakeBits(30), cronJob.Minutes.Val());
            Assert.AreEqual(MakeBits(2), cronJob.Hours.Val());
            Assert.AreEqual(MakeBits(0, 6), cronJob.Weekdays.Val());

            // @reboot cmd /c echo Run when the service is started (typically on reboot)
            cronJob = cronJobs[8];
            Assert.AreEqual("cmd /c echo Run when the service is started (typically on reboot)", cronJob.Command);
            Assert.IsTrue(cronJob.Reboot);

            Assert.AreEqual("c:\\examples", cronJob.Settings.Home);
            Assert.AreEqual(180, cronJob.Settings.Timeout);
            Assert.AreEqual("cronical@example.com", cronJob.Settings.MailFrom);
            Assert.AreEqual("admin@example.com", cronJob.Settings.MailTo);
            Assert.AreEqual(true, cronJob.Settings.MailStdOut);
            Assert.AreEqual("cc@example.com", cronJob.Settings.MailCc);
            Assert.AreEqual("bcc@example.com", cronJob.Settings.MailBcc);
            Assert.AreEqual("mail.example.com", cronJob.Settings.SmtpHost);
            Assert.AreEqual(false, cronJob.Settings.SmtpSSL);
            Assert.AreEqual("root@example.com", cronJob.Settings.SmtpUser);
            Assert.AreEqual("password", cronJob.Settings.SmtpPass);

            // @yearly cmd /c echo Once a year, typically midnight Jan 1st
            cronJob = cronJobs[9];
            Assert.AreEqual("cmd /c echo Once a year, typically midnight Jan 1st", cronJob.Command);
            Assert.AreEqual(MakeBits(0), cronJob.Minutes.Val());
            Assert.AreEqual(MakeBits(0), cronJob.Hours.Val());
            Assert.AreEqual(MakeBits(0), cronJob.Days.Val());
            Assert.AreEqual(MakeBits(0), cronJob.Months.Val());
            Assert.AreEqual(MakeAllBits(7), cronJob.Weekdays.Val());

            // @annually cmd /c echo Same as @yearly
            cronJob = cronJobs[10];
            Assert.AreEqual("cmd /c echo Same as @yearly", cronJob.Command);
            Assert.AreEqual(MakeBits(0), cronJob.Minutes.Val());
            Assert.AreEqual(MakeBits(0), cronJob.Hours.Val());
            Assert.AreEqual(MakeBits(0), cronJob.Days.Val());
            Assert.AreEqual(MakeBits(0), cronJob.Months.Val());
            Assert.AreEqual(MakeAllBits(7), cronJob.Weekdays.Val());

            // @monthly cmd /c echo Midnight on the first of every month
            cronJob = cronJobs[11];
            Assert.AreEqual("cmd /c echo Midnight on the first of every month", cronJob.Command);
            Assert.AreEqual(MakeBits(0), cronJob.Minutes.Val());
            Assert.AreEqual(MakeBits(0), cronJob.Hours.Val());
            Assert.AreEqual(MakeBits(0), cronJob.Days.Val());
            Assert.AreEqual(MakeAllBits(12), cronJob.Months.Val());
            Assert.AreEqual(MakeAllBits(7), cronJob.Weekdays.Val());

            // @weekly cmd /c echo Midnight on every Sunday
            cronJob = cronJobs[12];
            Assert.AreEqual("cmd /c echo Midnight on every Sunday", cronJob.Command);
            Assert.AreEqual(MakeBits(0), cronJob.Minutes.Val());
            Assert.AreEqual(MakeBits(0), cronJob.Hours.Val());
            Assert.AreEqual(MakeAllBits(31), cronJob.Days.Val());
            Assert.AreEqual(MakeAllBits(12), cronJob.Months.Val());
            Assert.AreEqual(MakeBits(0), cronJob.Weekdays.Val());

            // @daily cmd /c echo Midnight every day
            cronJob = cronJobs[13];
            Assert.AreEqual("cmd /c echo Midnight every day", cronJob.Command);
            Assert.AreEqual(MakeBits(0), cronJob.Minutes.Val());
            Assert.AreEqual(MakeBits(0), cronJob.Hours.Val());
            Assert.AreEqual(MakeAllBits(31), cronJob.Days.Val());
            Assert.AreEqual(MakeAllBits(12), cronJob.Months.Val());
            Assert.AreEqual(MakeAllBits(7), cronJob.Weekdays.Val());

            // @hourly cmd /c echo Every hour
            cronJob = cronJobs[14];
            Assert.AreEqual("cmd /c echo Every hour", cronJob.Command);
            Assert.AreEqual(MakeBits(0), cronJob.Minutes.Val());
            Assert.AreEqual(MakeAllBits(24), cronJob.Hours.Val());
            Assert.AreEqual(MakeAllBits(31), cronJob.Days.Val());
            Assert.AreEqual(MakeAllBits(12), cronJob.Months.Val());
            Assert.AreEqual(MakeAllBits(7), cronJob.Weekdays.Val());

            var serviceJobs = jobs.OfType<ServiceJob>().ToArray();
            Assert.AreEqual(2, serviceJobs.Length);
            Assert.AreEqual("cmd /k echo Start and keep running", serviceJobs[0].Command);
            Assert.AreEqual("cmd /k echo Start again and keep running", serviceJobs[1].Command);
        }

        [TestMethod]
        public void TestMakeBits()
        {
            Assert.AreEqual((ulong)0b00111111, MakeAllBits(6));
            Assert.AreEqual((ulong)0b00001010, MakeBits(1, 3));
        }

        private static ulong MakeBits(params int[] bits)
        {
            return bits.Aggregate<int, ulong>(0, (current, bit) => current | ((ulong)1 << bit));
        }

        private static ulong MakeAllBits(int count)
        {
            ulong result = 0;

            for (int i = 0; i < count; i++)
                result |= (ulong)1 << i;

            return result;
        }
    }
}
