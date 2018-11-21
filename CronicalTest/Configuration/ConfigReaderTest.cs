using System;
using Cronical.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cronical.Test.Configuration
{
    [TestClass]
    public class EnvironmentTest
    {
        [TestMethod]
        public void TestTrimComment()
        {
            Assert.AreEqual("", ConfigReader.TrimComment(null));
            Assert.AreEqual("", ConfigReader.TrimComment(""));
            Assert.AreEqual("", ConfigReader.TrimComment("   "));
            Assert.AreEqual("Hello", ConfigReader.TrimComment(" Hello "));
            Assert.AreEqual("", ConfigReader.TrimComment("#"));
            Assert.AreEqual("", ConfigReader.TrimComment("##"));
            Assert.AreEqual("Text", ConfigReader.TrimComment("Text # Comment"));
            Assert.AreEqual("Text", ConfigReader.TrimComment("Text # Comment # Again"));
            Assert.AreEqual("Text # More Text", ConfigReader.TrimComment("Text \\# More Text # Comment"));
            Assert.AreEqual("Text", ConfigReader.TrimComment("Text # Comment \\"));
        }
    }
}
