using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cronical.Test
{
  [TestClass]
  public class CronManagerTest
  {
    [TestMethod]
    [DeploymentItem("test.dat")]
    public void TestReadCronDat()
    {
      var mgr = new CronManager("test.dat");

      Assert.AreEqual(7, mgr.Config.CronJobs.Count());
      Assert.AreEqual(1, mgr.Config.ServiceJobs.Count());
    }

    [TestMethod]
    [DeploymentItem("test.dat")]
    public void TestReloadCronDat()
    {
      var mgr = new CronManager("test.dat");

      Assert.AreEqual(7, mgr.Config.CronJobs.Count());
      Assert.AreEqual(1, mgr.Config.ServiceJobs.Count());

      // Reload with config changed = true
      mgr.Reload();

      Assert.AreEqual(7, mgr.Config.CronJobs.Count());
      Assert.AreEqual(1, mgr.Config.ServiceJobs.Count());
    }
  }
}
