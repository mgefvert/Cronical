using System;
using Cronical.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cronical.Test.Configuration
{
    [TestClass]
    public class SettingsTest
    {
        [TestMethod]
        public void TestClone()
        {
            var env = new JobSettings
            {
                Home = "home",
                MailTo = "to-email",
                MailFrom = "from-email",
                SmtpHost = "server",
                SmtpPass = "password",
                SmtpUser = "user"
            };

            var env2 = env.Clone();

            Assert.AreNotSame(env, env2);
            Assert.AreEqual(env.Home, env2.Home);
            Assert.AreEqual(env.MailFrom, env2.MailFrom);
            Assert.AreEqual(env.MailTo, env2.MailTo);
            Assert.AreEqual(env.SmtpHost, env2.SmtpHost);
            Assert.AreEqual(env.SmtpPass, env2.SmtpPass);
            Assert.AreEqual(env.SmtpUser, env2.SmtpUser);
        }

        [TestMethod]
        public void TestExists()
        {
            var env = new JobSettings();

            Assert.IsTrue(env.Exists("home"));
            Assert.IsTrue(env.Exists("MAILTO"));
            Assert.IsTrue(env.Exists("SmtpUser"));
            Assert.IsFalse(env.Exists("NotExists"));
        }

        [TestMethod]
        public void TestSet()
        {
            var env = new JobSettings();

            env.Set("home", "bork");
            env.Set("MAILTO", "xxx");

            Assert.AreEqual("bork", env.Home);
            Assert.AreEqual("xxx", env.MailTo);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception), AllowDerivedTypes = true)]
        public void TestSetInvalid()
        {
            var env = new JobSettings();
            env.Set("xxx", "bork");
        }
    }
}
