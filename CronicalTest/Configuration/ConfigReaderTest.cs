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
            Assert.AreEqual("", ConfigReader.PreprocessLine(null));
            Assert.AreEqual("", ConfigReader.PreprocessLine(""));
            Assert.AreEqual("", ConfigReader.PreprocessLine("   "));
            Assert.AreEqual("Hello", ConfigReader.PreprocessLine(" Hello "));
            Assert.AreEqual("", ConfigReader.PreprocessLine("#"));
            Assert.AreEqual("", ConfigReader.PreprocessLine("##"));
            Assert.AreEqual("Text", ConfigReader.PreprocessLine("Text # Comment"));
            Assert.AreEqual("Text", ConfigReader.PreprocessLine("Text # Comment # Again"));
            Assert.AreEqual("Text # More Text", ConfigReader.PreprocessLine("Text \\# More Text # Comment"));
            Assert.AreEqual("Text", ConfigReader.PreprocessLine("Text # Comment \\"));
        }
    }
}
