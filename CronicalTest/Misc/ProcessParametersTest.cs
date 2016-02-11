using System;
using Cronical.Misc;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cronical.Test.Misc
{
  [TestClass]
  public class ProcessParametersTest
  {
    [TestMethod]
    public void Test()
    {
      var cmd = new ProcessParameters("mytest.exe one two three", "c:\\");
      Assert.AreEqual("c:\\mytest.exe", cmd.Executable);
      Assert.AreEqual("c:\\", cmd.Directory);
      Assert.AreEqual("one two three", cmd.Parameters);

      cmd = new ProcessParameters("mytest.exe", "c:\\windows\\system32");
      Assert.AreEqual("c:\\windows\\system32\\mytest.exe", cmd.Executable);
      Assert.AreEqual("c:\\windows\\system32", cmd.Directory);
      Assert.AreEqual("", cmd.Parameters);

      cmd = new ProcessParameters(".\\mytest.exe", "c:\\windows\\system32");
      Assert.AreEqual("c:\\windows\\system32\\mytest.exe", cmd.Executable);
      Assert.AreEqual("c:\\windows\\system32", cmd.Directory);
      Assert.AreEqual("", cmd.Parameters);

      cmd = new ProcessParameters("..\\mytest.exe", "c:\\windows\\system32");
      Assert.AreEqual("c:\\windows\\mytest.exe", cmd.Executable);
      Assert.AreEqual("c:\\windows", cmd.Directory);
      Assert.AreEqual("", cmd.Parameters);

      cmd = new ProcessParameters("..\\..\\mytest.exe", "c:\\windows\\system32");
      Assert.AreEqual("c:\\mytest.exe", cmd.Executable);
      Assert.AreEqual("c:\\", cmd.Directory);
      Assert.AreEqual("", cmd.Parameters);

      cmd = new ProcessParameters("c:\\mytest.exe", "c:\\windows\\system32");
      Assert.AreEqual("c:\\mytest.exe", cmd.Executable);
      Assert.AreEqual("c:\\", cmd.Directory);
      Assert.AreEqual("", cmd.Parameters);

      cmd = new ProcessParameters("c:\\local\\mytest.exe", "c:\\windows\\system32");
      Assert.AreEqual("c:\\local\\mytest.exe", cmd.Executable);
      Assert.AreEqual("c:\\local", cmd.Directory);
      Assert.AreEqual("", cmd.Parameters);
    }

    [TestMethod]
    public void TestCmd()
    {
      var cmd = new ProcessParameters("cmd /c dir", "c:\\");
      Assert.AreEqual("c:\\windows\\system32\\cmd.exe", cmd.Executable.ToLower());
      Assert.AreEqual("c:\\", cmd.Directory);
      Assert.AreEqual("/c dir", cmd.Parameters);
    }
  }
}
