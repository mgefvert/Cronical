using System;
using Cronical.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cronical.Test.Configuration
{
    public class SampleTest : AbstractSettings
    {
        public string S { get; set; }
        public int N { get; set; }
        public bool B { get; set; }
    }

    [TestClass]
    public class AbstractSettingsTest
    {
        private SampleTest _x;

        [TestInitialize]
        public void Setup()
        {
            _x = new SampleTest();
        }

        [TestMethod]
        public void TestExists()
        {
            Assert.IsTrue(_x.Exists("s"));
            Assert.IsTrue(_x.Exists("n"));
            Assert.IsTrue(_x.Exists("b"));
            Assert.IsFalse(_x.Exists("x"));
            Assert.IsTrue(_x.Exists("S"));
            Assert.IsTrue(_x.Exists("N"));
            Assert.IsTrue(_x.Exists("B"));
            Assert.IsFalse(_x.Exists("X"));
            Assert.IsFalse(_x.Exists(""));
            Assert.IsFalse(_x.Exists(null));
        }

        [TestMethod]
        public void TestSet()
        {
            Assert.IsTrue(_x.Set("s", "hello"));
            Assert.IsTrue(_x.Set("n", "42"));
            Assert.IsTrue(_x.Set("b", "true"));
            Assert.IsFalse(_x.Set("x", "bork"));

            Assert.AreEqual("hello", _x.S);
            Assert.AreEqual(42, _x.N);
            Assert.AreEqual(true, _x.B);
        }

        [TestMethod]
        public void TestToString()
        {
            _x.S = "hello";
            _x.N = 42;

            Assert.AreEqual("hello,42,False", _x.ToString());
        }
    }
}
