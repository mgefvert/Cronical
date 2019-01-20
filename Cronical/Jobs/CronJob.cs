using System;
using System.Collections;
using System.Linq;
using System.Threading;
using Cronical.Misc;
using DotNetCommons.Logging;

namespace Cronical.Jobs
{
    public class CronJob : Job
    {
        protected static readonly DateTime Never = new DateTime(9999, 1, 1);

        public BitArray Weekdays = new BitArray(7);
        public BitArray Months = new BitArray(12);
        public BitArray Days = new BitArray(31);
        public BitArray Hours = new BitArray(24);
        public BitArray Minutes = new BitArray(60);
        public bool Reboot;
        public DateTime NextExecTime;

        public override string GetJobCode()
        {
            var values = new[] { Weekdays, Months, Days, Hours, Minutes }.Select(x => x.Val().ToString());

            return base.GetJobCode() + "," + (Reboot ? "reboot" : "") + "," + string.Join(",", values);
        }

        public override void RecalcNextExecTime()
        {
            RecalcNextExecTime(DateTime.Now);
        }

        public void RecalcNextExecTime(DateTime origin)
        {
            if (Reboot)
            {
                NextExecTime = Never;
                Logger.Debug("Next job start :: at boot :: for " + Command);
                return;
            }

            // Start at the given time plus one minute.
            var test = origin.AddSeconds(60 - origin.Second);
            if (test.Minute == origin.Minute)
                // Failsafe - just in case AddSeconds didn't advance one minute... :)
                test = test.AddMinutes(1);

            // Find the end search time - the origin plus one year.
            var end = origin.AddYears(1);

            // Loop through every single minute of the search space and test for execution.
            while (test < end)
            {
                if (Minutes.Get(test.Minute) && Hours.Get(test.Hour) && Days.Get(test.Day - 1) &&
                    Months.Get(test.Month - 1) && Weekdays.Get((int)test.DayOfWeek))
                {
                    NextExecTime = test;
                    Logger.Debug($"Next job start {NextExecTime} for {Command}");
                    return;
                }

                test = test.AddMinutes(1);
            }

            // This shouldn't really happen.
            NextExecTime = Never;
            Logger.Warning("No start time for job found: " + Command);
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
