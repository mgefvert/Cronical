using System;
using Cronical.Configuration;
using Cronical.Jobs;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cronical.Test.Jobs
{
  [TestClass]
  public class ServiceJobTest
  {
    [TestMethod]
    public void TestGetCode()
    {
      var job = new ServiceJob
      {
        Command = "process.exe",
        Settings = new Settings
        {
          Home = "c:\\windows",
          Timeout = 86400
        }
      };

      Assert.AreEqual("ServiceJob,process.exe,c:\\windows,False,,,,,False,,,False,,86400", job.GetJobCode());
    }
  }
}
