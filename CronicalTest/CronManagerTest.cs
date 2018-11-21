using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cronical.Test
{
    [TestClass]
    public class CronManagerTest
    {
        private string ProcessWhitespace(string s)
        {
            s = s.Replace("\t", " ");
            while (s.Contains("  "))
                s = s.Replace("  ", " ");

            return s.Trim();
        }

        [TestMethod]
        [DeploymentItem("test.dat")]
        public void TestReadCronDat()
        {
            var mgr = new CronManager("test.dat");

            Assert.AreEqual(8, mgr.Config.CronJobs.Count());
            Assert.AreEqual(1, mgr.Config.ServiceJobs.Count());

            var job = mgr.Config.CronJobs.SingleOrDefault(x => x.Command.Contains("This should be visible"));
            Assert.IsNotNull(job);
            Assert.AreEqual("cmd /c echo # This should be visible", ProcessWhitespace(job.Command));
        }

        [TestMethod]
        [DeploymentItem("test.dat")]
        public void TestReloadCronDat()
        {
            var mgr = new CronManager("test.dat");

            Assert.AreEqual(8, mgr.Config.CronJobs.Count());
            Assert.AreEqual(1, mgr.Config.ServiceJobs.Count());

            // Reload with config changed = true
            mgr.Reload();

            Assert.AreEqual(8, mgr.Config.CronJobs.Count());
            Assert.AreEqual(1, mgr.Config.ServiceJobs.Count());
        }
    }
}
