using System;
using System.Threading;
using Cronical.Misc;
using DotNetCommons.Logging;

namespace Cronical.Jobs
{
    public class SingleJob : Job
    {
        public override void RecalcNextExecTime()
        {
        }

        public void Run()
        {
            RecalcNextExecTime();

            new Thread(() => RunJobThread()).Start();
        }

        public string RunJobThread()
        {
            string output = null;
            try
            {
                var process = new ProcessWrapper(Command, Settings.Home, true, Settings.MailStdOut);

                Logger.Log("Starting job: " + Command);
                process.Start();

                Logger.Debug($"Process started, waiting at most {Settings.Timeout} seconds");
                process.WaitForEnd(Settings.Timeout * 1000);
                output = process.FetchResult();

                Helper.SendMail("Cronical: Results from " + Command, output, Settings);
            }
            catch (Exception e)
            {
                var text = $"Failed to start process '{Command}': {e.Message}";
                Logger.Error(text);
                Helper.SendMail("Cronical: Failed to start " + Command, text, Settings);
            }

            Logger.Debug("Job finished");
            return output;
        }
    }
}
