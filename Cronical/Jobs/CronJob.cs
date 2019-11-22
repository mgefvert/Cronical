using System;
using System.Collections;
using System.Linq;
using Cronical.Misc;
using DotNetCommons.Logging;

namespace Cronical.Jobs
{
    /// <summary>
    /// Class that handles a cron job that is to be run at regular intervals.
    /// </summary>
    public class CronJob : SingleJob
    {
        protected static readonly DateTime Never = new DateTime(9999, 1, 1);

        // Bit field arrays describing when the job is to be run.
        public BitArray Weekdays = new BitArray(7);
        public BitArray Months = new BitArray(12);
        public BitArray Days = new BitArray(31);
        public BitArray Hours = new BitArray(24);
        public BitArray Minutes = new BitArray(60);
        public bool Reboot;

        // Calculated next execution time.
        public DateTime NextExecTime;

        /// <summary>
        /// Job signature that is used to determine equality between jobs.
        /// </summary>
        /// <returns></returns>
        public override string GetJobCode()
        {
            var values = new[] { Weekdays, Months, Days, Hours, Minutes }.Select(x => x.Val().ToString());

            return base.GetJobCode() + "," + (Reboot ? "reboot" : "") + "," + string.Join(",", values);
        }

        public override void RecalcNextExecTime()
        {
            RecalcNextExecTime(DateTime.Now);
        }

        /// <summary>
        /// Recalculate the next start time. Simply iterate through every given minute up to
        /// one year out and check for hits. Yields a maximum of 525600 tests, which is not
        /// difficult for a modern computer.
        /// </summary>
        /// <param name="origin"></param>
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
    }
}
