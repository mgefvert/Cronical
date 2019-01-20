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
        private SampleTest x;

        [TestInitialize]
        public void Setup()
        {
            x = new SampleTest();
        }

        [TestMethod]
        public void TestExists()
        {
            Assert.IsTrue(x.Exists("s"));
            Assert.IsTrue(x.Exists("n"));
            Assert.IsTrue(x.Exists("b"));
            Assert.IsFalse(x.Exists("x"));
            Assert.IsTrue(x.Exists("S"));
            Assert.IsTrue(x.Exists("N"));
            Assert.IsTrue(x.Exists("B"));
            Assert.IsFalse(x.Exists("X"));
            Assert.IsFalse(x.Exists(""));
            Assert.IsFalse(x.Exists(null));
        }

        [TestMethod]
        public void TestSet()
        {
            Assert.IsTrue(x.Set("s", "hello"));
            Assert.IsTrue(x.Set("n", "42"));
            Assert.IsTrue(x.Set("b", "true"));
            Assert.IsFalse(x.Set("x", "bork"));

            Assert.AreEqual("hello", x.S);
            Assert.AreEqual(42, x.N);
            Assert.AreEqual(true, x.B);
        }

        [TestMethod]
        public void TestToString()
        {
            x.S = "hello";
            x.N = 42;

            Assert.AreEqual("hello,42,False", x.ToString());
        }
    }
}
