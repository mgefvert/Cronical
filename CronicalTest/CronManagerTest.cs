using System;
using System.Linq;
using Cronical.Configuration;
using Cronical.Integrations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cronical.Test
{
    [TestClass]
    public class CronManagerTest
    {
        private CronManager _manager;

        [TestInitialize]
        public void Setup()
        {
            _manager = new CronManager(new GlobalSettings(), new JobSettings(), new [] { new FileConfigReader("test.dat") });
        }

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
            Assert.AreEqual(8, _manager.CronJobs.Count());
            Assert.AreEqual(1, _manager.ServiceJobs.Count());

            var job = _manager.CronJobs.SingleOrDefault(x => x.Command.Contains("This should be visible"));
            Assert.IsNotNull(job);
            Assert.AreEqual("cmd /c echo # This should be visible", ProcessWhitespace(job.Command));
        }

        [TestMethod]
        [DeploymentItem("test.dat")]
        public void TestReloadCronDat()
        {
            Assert.AreEqual(8, _manager.CronJobs.Count());
            Assert.AreEqual(1, _manager.ServiceJobs.Count());

            // Reload with config changed = true
            _manager.Reload();

            Assert.AreEqual(8, _manager.CronJobs.Count());
            Assert.AreEqual(1, _manager.ServiceJobs.Count());
        }
    }
}
