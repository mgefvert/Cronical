using System;
using Cronical.Configuration;
using DotNetCommons.Sys;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cronical.Test.Configuration
{
    [TestClass]
    public class CommandLineOptionsTest
    {
        [TestInitialize]
        public void Setup()
        {
            CommandLine.DisplayHelpOnEmpty = false;
        }

        [TestMethod]
        public void TestNone()
        {
            var cmd = CommandLine.Parse<CommandLineOptions>(new string[0]);

            Assert.AreEqual("cronical.dat", cmd.ConfigFile);
            Assert.IsFalse(cmd.ConfigFileOverride);
            Assert.IsFalse(cmd.DebugLogs);
            Assert.IsFalse(cmd.Help);
            Assert.IsFalse(cmd.InstallService);
            Assert.IsFalse(cmd.RemoveService);
            Assert.IsFalse(cmd.RunAsConsole);
            Assert.AreEqual("Cronical", cmd.ServiceName);
            Assert.AreEqual("Cronical Job Scheduler", cmd.ServiceTitle);
            Assert.AreEqual(null, cmd.ServiceDescription);
        }

        [TestMethod]
        public void TestMany()
        {
            var cmd = CommandLine.Parse<CommandLineOptions>("-d", "--install", "--remove", "--console", "-c", "test.dat", "-h", 
                "--service-name=c1", "--service-title=\"Cronical 1\"", "--service-desc=\"Cronical instance 1\"");

            Assert.AreEqual("test.dat", cmd.ConfigFile);
            Assert.IsTrue(cmd.ConfigFileOverride);
            Assert.IsTrue(cmd.DebugLogs);
            Assert.IsTrue(cmd.Help);
            Assert.IsTrue(cmd.InstallService);
            Assert.IsTrue(cmd.RemoveService);
            Assert.IsTrue(cmd.RunAsConsole);
            Assert.AreEqual("c1", cmd.ServiceName);
            Assert.AreEqual("Cronical 1", cmd.ServiceTitle);
            Assert.AreEqual("Cronical instance 1", cmd.ServiceDescription);
        }

        
        [TestMethod]
        public void TestHelp()
        {
            Assert.IsTrue(CommandLine.Parse<CommandLineOptions>("-h").Help);
            Assert.IsTrue(CommandLine.Parse<CommandLineOptions>("--help").Help);
            Assert.IsTrue(CommandLine.Parse<CommandLineOptions>("-?").Help);
            Assert.IsTrue(CommandLine.Parse<CommandLineOptions>("/h").Help);
        }

        [TestMethod]
        public void TestOverride()
        {
            var cmd = CommandLine.Parse<CommandLineOptions>("--config=test.dat");
            Assert.AreEqual("test.dat", cmd.ConfigFile);
            Assert.IsTrue(cmd.ConfigFileOverride);

            cmd = CommandLine.Parse<CommandLineOptions>("--config", "test.dat");
            Assert.AreEqual("test.dat", cmd.ConfigFile);
            Assert.IsTrue(cmd.ConfigFileOverride);

            cmd = CommandLine.Parse<CommandLineOptions>("-c", "test.dat");
            Assert.AreEqual("test.dat", cmd.ConfigFile);
            Assert.IsTrue(cmd.ConfigFileOverride);
        }
    }
}
