using System;
using Cronical.Configuration;
using Cronical.Misc;
using DotNetCommons.Logging;

namespace Cronical.Jobs
{
    public class ServiceJob : Job
    {
        protected ProcessWrapper Process;
        public JobState State { get; private set; }

        public bool CheckIsRunning()
        {
            var result = Process != null && Process.Running;

            if (State == JobState.Running && result == false)
            {
                Logger.Warning($"Service terminated unexpectedly: '{Command}'");
                State = JobState.Inactive;
            }

            return result;
        }

        public void Run()
        {
            CheckIsRunning();
            switch (State)
            {
                case JobState.Starting:
                    Logger.Warning($"Run: Service is already starting: '{Command}'");
                    return;

                case JobState.Running:
                    Logger.Warning($"Run: Job is already running: '{Command}'");
                    return;

                case JobState.Stopping:
                    Logger.Warning($"Run: Unable to start, job is stopping: '{Command}'");
                    return;
            }

            State = JobState.Starting;
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
            finally
            {
                State = CheckIsRunning() ? JobState.Running : JobState.Inactive;
            }
        }

        public void Terminate()
        {
            CheckIsRunning();

            switch (State)
            {
                case JobState.Starting:
                case JobState.Running:
                    Logger.Log("Terminating service: " + Command);
                    Process.Stop();
                    return;

                case JobState.Stopping:
                    Logger.Warning($"Run: Job is already stopping: '{Command}'");
                    return;
            }
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
