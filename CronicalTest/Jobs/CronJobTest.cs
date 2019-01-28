using System;
using Cronical.Configuration;
using Cronical.Integrations;
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
            var job = FileConfigReader.ParseJob("0,15,30,45 * * */2 * process.exe", new JobSettings
            {
                Home = "c:\\windows",
                Timeout = 86400
            });

            Assert.AreEqual("CronJob,process.exe,c:\\windows,False,,,,,,,False,,86400,,127,1365,2147483647,16777215,35185445863425", job.GetJobCode());
        }
    }
}
