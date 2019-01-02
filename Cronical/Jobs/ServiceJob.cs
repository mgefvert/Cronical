using System;
using Cronical.Configuration;
using Cronical.Misc;
using DotNetCommons.Logging;

namespace Cronical.Jobs
{
    public class ServiceJob : Job
    {
        protected ProcessWrapper Process;

        public bool IsRunning()
        {
            return Process != null && Process.Running;
        }

        public void Run()
        {
            Process = new ProcessWrapper(Command, Settings.Home, false, false);

            try
            {
                Logger.Log("Starting service: " + Command);
                Process.Start();
                Logger.Debug("Process started");
            }
            catch (Exception e)
            {
                var text = $"Failed to start service '{Command}': {e.Message}";
                Logger.Error(text);
                Helper.SendMail("Cronical: Failed to start service " + Command, text, Settings);
            }
        }

        public void Terminate()
        {
            Logger.Log("Terminating service: " + Command);

            if (!IsRunning())
                return;

            Process.Stop();
        }

        public static ServiceJob Parse(ConfigReader.JobArgs jobArgs, Settings settings = null)
        {
            if (jobArgs == null)
                return null;

            return new ServiceJob
            {
                Settings = settings != null ? settings.Clone() : new Settings(),
                Command = jobArgs.Command
            };
        }
    }
}
