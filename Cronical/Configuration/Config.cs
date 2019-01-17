using System;
using System.Collections.Generic;
using System.Linq;
using Cronical.Jobs;
using DotNetCommons.Logging;

namespace Cronical.Configuration
{
    public class Config
    {
        public List<Job> Jobs { get; } = new List<Job>();
        public GlobalSettings Settings { get; set; } = new GlobalSettings();

        public IEnumerable<CronJob> CronJobs => Jobs.OfType<CronJob>();
        public IEnumerable<ServiceJob> ServiceJobs => Jobs.OfType<ServiceJob>();

        public void DisplaySettingsInfo()
        {
            Logger.Log($"Config: Run missed jobs on startup = {Settings.RunMissedJobs}");
            Logger.Log($"Config: Check services every       = {Settings.ServiceChecks} seconds" + (Settings.ServiceChecks == 0 ? " (constantly)" : ""));
            Logger.Log($"Config: Terminate cron jobs after  = {Settings.Timeout} seconds");
        }
    }
}
