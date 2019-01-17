using System;
using System.IO;
using Cronical.Configuration;
using Cronical.Misc;
using DotNetCommons.Logging;

namespace Cronical.Jobs
{
    public abstract class Job
    {
        public string Command;
        public JobSettings Settings { get; set; }

        public virtual string GetJobCode()
        {
            return GetType().Name + "," + Command + "," + Settings;
        }

        public abstract void RecalcNextExecTime();

        public void VerifyExecutableExists()
        {
            var process = new ProcessParameters(Command, Settings.Home);
            if (!File.Exists(process.Executable))
                Logger.Warning($"File {process.Executable} does not seem to exist");
        }
    }
}
