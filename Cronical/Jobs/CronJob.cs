using System;
using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Cronical.Configuration;
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

        /// <summary>
        /// Builds a job-unique string that can be compared, to see which jobs may survive on reload
        /// </summary>
        public override string GetJobCode()
        {
            var values = new[] { Weekdays, Months, Days, Hours, Minutes }.Select(x => x.Val().ToString());

            return base.GetJobCode() + "," + (Reboot ? "reboot" : "") + "," + string.Join(",", values);
        }

        public void RecalcNextExecTime()
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

            var thread = new Thread(() => RunJobThread());
            thread.Start();
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

        public static CronJob Parse(ConfigReader.JobArgs jobArgs, Settings settings = null)
        {
            if (jobArgs == null)
                return null;

            var job = new CronJob { Settings = settings != null ? settings.Clone() : new Settings() };

            if (jobArgs.Reboot)
            {
                job.Reboot = true;
                job.Command = jobArgs.Command;
                return job;
            }

            try
            {
                ParseValue(ref job.Minutes, jobArgs.Minute, 0, 59, false);
                ParseValue(ref job.Hours, jobArgs.Hour, 0, 23, false);
                ParseValue(ref job.Days, jobArgs.Day, 1, 31, false);
                ParseValue(ref job.Months, jobArgs.Month, 1, 12, false);
                ParseValue(ref job.Weekdays, jobArgs.Weekday, 0, 6, true);

                job.Command = jobArgs.Command;
                job.RecalcNextExecTime();

                return job;
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                return null;
            }
        }

        private static void ParseValue(ref BitArray value, string spec, int min, int max, bool weekdays)
        {
            try
            {
                var regex = new Regex(@"^(([\d\w]+)(-([\d\w]+))?|\*)(/(\d+))?$");

                foreach (var s in spec.Split(','))
                {
                    var result = regex.Match(s);
                    if (!result.Success)
                        throw new Exception();

                    var star = result.Groups[1].Value == "*";
                    var start = result.Groups[2].Value;
                    var stop = result.Groups[4].Value;
                    var every = result.Groups[6].Value;

                    if (weekdays)
                    {
                        start = TranslateWeekDay(start);
                        stop = TranslateWeekDay(stop);
                    }

                    if (every == "")
                        every = "1";

                    int istart;
                    int istop;
                    var ievery = int.Parse(every);

                    if (star)
                    {
                        istart = min;
                        istop = max;
                    }
                    else
                    {
                        istart = int.Parse(start);
                        istop = int.Parse(stop == "" ? start : stop);
                    }

                    if (istart < min || istart > max || istop < min || istop > max || ievery < 1)
                        throw new Exception();

                    for (var i = istart - min; i <= istop - min; i += ievery)
                        value.Set(i, true);
                }
            }
            catch (Exception)
            {
                throw new Exception("Can't make sense of '" + spec + "'");
            }
        }

        private static string TranslateWeekDay(string start)
        {
            int dummy;

            if (start == "7")
                return "0";

            if (int.TryParse(start, out dummy) || start == "*" || start == "")
                return start;

            switch (start.ToLower())
            {
                case "sun":
                case "sunday":
                    return "0";
                case "mon":
                case "monday":
                    return "1";
                case "tue":
                case "tuesday":
                    return "2";
                case "wed":
                case "wednesday":
                    return "3";
                case "thu":
                case "thursday":
                    return "4";
                case "fri":
                case "friday":
                    return "5";
                case "sat":
                case "saturday":
                    return "6";

                default:
                    throw new Exception("Unrecognized weekday name: " + start);
            }
        }
    }
}
