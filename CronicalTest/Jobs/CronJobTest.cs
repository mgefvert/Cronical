using System;
using System.IO;
using System.Linq;
using Cronical.Configuration;
using Cronical.Jobs;
using Cronical.Misc;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable LocalizableElement

namespace Cronical.Test.Jobs
{
    [TestClass]
    public class CronJobTest
    {
        [TestMethod]
        public void TestGetCode()
        {
            var args = new ConfigReader.JobArgs
            {
                Command = "process.exe",
                Day = "*",
                Hour = "*",
                Minute = "0,15,30,45",
                Month = "*/2",
                Weekday = "*",
            };

            var job = CronJob.Parse(args, new JobSettings
            {
                Home = "c:\\windows",
                Timeout = 86400
            });

            Assert.AreEqual("CronJob,process.exe,c:\\windows,False,,,,,,,False,,,127,1365,2147483647,16777215,35185445863425", job.GetJobCode());
        }

        [TestMethod]
        public void TestStripComments()
        {
            Assert.IsNull(CronJob.Parse(ConfigReader.DoParseJobLine(null)));
            Assert.IsNull(CronJob.Parse(ConfigReader.DoParseJobLine("")));
            Assert.IsNull(CronJob.Parse(ConfigReader.DoParseJobLine("       ")));
            Assert.IsNull(CronJob.Parse(ConfigReader.DoParseJobLine("   # This is some whitespace")));
        }

        [TestMethod]
        public void TestSpecials()
        {
            CronJob job;

            job = CronJob.Parse(ConfigReader.DoParseJobLine("@reboot      Reboot"));
            Assert.AreEqual("Reboot", job.Command);
            Assert.IsTrue(job.Reboot);

            job = CronJob.Parse(ConfigReader.DoParseJobLine("@yearly yearly"));
            Assert.AreEqual("yearly", job.Command);
            Assert.AreEqual(1, job.Minutes.Val());
            Assert.AreEqual(1, job.Hours.Val());
            Assert.AreEqual(1, job.Days.Val());
            Assert.AreEqual(1, job.Months.Val());
            Assert.IsFalse(job.Reboot);

            job = CronJob.Parse(ConfigReader.DoParseJobLine("@annually annually"));
            Assert.AreEqual("annually", job.Command);
            Assert.AreEqual(1, job.Minutes.Val());
            Assert.AreEqual(1, job.Hours.Val());
            Assert.AreEqual(1, job.Days.Val());
            Assert.AreEqual(1, job.Months.Val());
            Assert.IsFalse(job.Reboot);

            job = CronJob.Parse(ConfigReader.DoParseJobLine("@monthly monthly"));
            Assert.AreEqual("monthly", job.Command);
            Assert.AreEqual(1, job.Minutes.Val());
            Assert.AreEqual(1, job.Hours.Val());
            Assert.AreEqual(1, job.Days.Val());
            Assert.IsFalse(job.Reboot);

            job = CronJob.Parse(ConfigReader.DoParseJobLine("@weekly weekly"));
            Assert.AreEqual("weekly", job.Command);
            Assert.AreEqual(1, job.Minutes.Val());
            Assert.AreEqual(1, job.Hours.Val());
            Assert.AreEqual(1, job.Weekdays.Val());
            Assert.IsFalse(job.Reboot);

            job = CronJob.Parse(ConfigReader.DoParseJobLine("@daily daily"));
            Assert.AreEqual("daily", job.Command);
            Assert.AreEqual(1, job.Minutes.Val());
            Assert.AreEqual(1, job.Hours.Val());
            Assert.IsFalse(job.Reboot);

            job = CronJob.Parse(ConfigReader.DoParseJobLine("@hourly hourly"));
            Assert.AreEqual("hourly", job.Command);
            Assert.AreEqual(1, job.Minutes.Val());
            Assert.IsFalse(job.Reboot);
        }

        [TestMethod]
        public void TestNormal()
        {
            CronJob job;

            job = CronJob.Parse(ConfigReader.DoParseJobLine(" * * * * *    xx"));
            Assert.AreEqual("xx", job.Command);
            Assert.AreEqual(MakeAllBits(24), job.Hours.Val());
            Assert.AreEqual(MakeAllBits(31), job.Days.Val());
            Assert.AreEqual(MakeAllBits(12), job.Months.Val());
            Assert.AreEqual(MakeAllBits(60), job.Minutes.Val());
            // Assert.AreEqual(MakeAllBits(7), job.Weekdays.Val());
            Assert.IsFalse(job.Reboot);

            job = CronJob.Parse(ConfigReader.DoParseJobLine("* 12 3 5 *    xx"));
            Assert.AreEqual("xx", job.Command);
            Assert.AreEqual(MakeBits(0, 12), job.Hours.Val());
            Assert.AreEqual(MakeBits(1, 3), job.Days.Val());
            Assert.AreEqual(MakeBits(1, 5), job.Months.Val());
            Assert.IsFalse(job.Reboot);

            job = CronJob.Parse(ConfigReader.DoParseJobLine("0-2,7-9 * * * *    x x"));
            Assert.AreEqual("x x", job.Command);
            Assert.AreEqual(MakeBits(0, 0, 1, 2, 7, 8, 9), job.Minutes.Val());
            Assert.IsFalse(job.Reboot);

            job = CronJob.Parse(ConfigReader.DoParseJobLine("0 */2 * * *       Every other hour"));
            Assert.AreEqual("Every other hour", job.Command);
            Assert.AreEqual(1, job.Minutes.Val());
            Assert.AreEqual(MakeBits(0, 0, 2, 4, 6, 8, 10, 12, 14, 16, 18, 20, 22), job.Hours.Val());
            Assert.IsFalse(job.Reboot);

            job = CronJob.Parse(ConfigReader.DoParseJobLine("0 9-17/3 * * *    From 09:00 - 17:00, every third hour"));
            Assert.AreEqual("From 09:00 - 17:00, every third hour", job.Command);
            Assert.AreEqual(1, job.Minutes.Val());
            Assert.AreEqual(MakeBits(0, 9, 12, 15), job.Hours.Val());
            Assert.IsFalse(job.Reboot);

            job = CronJob.Parse(ConfigReader.DoParseJobLine("0 8,12,17 * * *   At 08:00, 12:00, 17:00"));
            Assert.AreEqual("At 08:00, 12:00, 17:00", job.Command);
            Assert.AreEqual(1, job.Minutes.Val());
            Assert.AreEqual(MakeBits(0, 8, 12, 17), job.Hours.Val());
            Assert.IsFalse(job.Reboot);
        }

        [TestMethod]
        public void TestSpacing()
        {
            var job = CronJob.Parse(ConfigReader.DoParseJobLine(" * * * * *    xx\t1\t2"));
            Assert.AreEqual("xx 1 2", job.Command);
        }

        private static Int64 MakeBits(int offset, params int[] bits)
        {
            return bits.Aggregate<int, Int64>(0, (current, bit) => current | ((Int64)1 << (bit - offset)));
        }

        private static Int64 MakeAllBits(int count)
        {
            Int64 result = 0;

            for (int i = 0; i < count; i++)
                result |= (Int64)1 << i;

            return result;
        }
    }
}
