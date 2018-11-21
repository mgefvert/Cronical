using System;
using System.IO;
using Cronical.Misc;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cronical.Test.Misc
{
    [TestClass]
    public class ProcessWrapperTest
    {
        private string _path;

        [TestInitialize]
        public void Setup()
        {
            _path = Directory.GetCurrentDirectory();
        }

        [TestMethod]
        public void TestStart_DefaultPath()
        {
            var wrapper = new ProcessWrapper("cmd /c cd", _path, false, true);
            wrapper.Start();
            wrapper.WaitForEnd(5000);

            var result = wrapper.FetchResult().Trim();

            Assert.AreEqual(Directory.GetCurrentDirectory().ToLower(), result.ToLower());
        }

        [TestMethod]
        public void TestStart_GivenPath()
        {
            var wrapper = new ProcessWrapper("c:\\windows\\system32\\cmd /c cd", _path, false, true);
            wrapper.Start();
            wrapper.WaitForEnd(5000);

            var result = wrapper.FetchResult().Trim();

            Assert.AreEqual("c:\\windows\\system32".ToLower(), result.ToLower());
        }

        [TestMethod]
        public void TestStart_RelativePath()
        {
            var wrapper = new ProcessWrapper("system32\\cmd /c cd", "c:\\windows", false, true);
            wrapper.Start();
            wrapper.WaitForEnd(5000);

            var result = wrapper.FetchResult().Trim();

            Assert.AreEqual("c:\\windows\\system32".ToLower(), result.ToLower());
        }
    }
}
