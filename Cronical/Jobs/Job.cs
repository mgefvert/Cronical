using System;
using System.IO;
using Cronical.Configuration;
using Cronical.Misc;
using DotNetCommons.Logging;

namespace Cronical.Jobs
{
    public class Job
    {
        public string Command;
        public Settings Settings;

        public void VerifyExecutableExists()
        {
            var process = new ProcessParameters(Command, Settings.Home);
            if (!File.Exists(process.Executable))
                Logger.Warning($"File {process.Executable} does not seem to exist");
        }

        public virtual string GetJobCode()
        {
            return GetType().Name + "," + Command + "," + Settings;
        }
    }
}
