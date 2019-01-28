using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Cronical.Configuration;
using Cronical.Integrations;
using Cronical.Jobs;
using DotNetCommons.Collections;
using DotNetCommons.Logging;
using Microsoft.Win32;

namespace Cronical
{
    public class CronManager
    {
        public const string RegKey = @"HKEY_CURRENT_USER\Software\Ciceronen\Cronical";

        // Global configuration
        private readonly IIntegration[] _integrations;
        private readonly GlobalSettings _settings;

        // Timers and locks
        private DateTime _lastDate;
        private DateTime _lastService;
        private volatile bool _inTick;
        private readonly object _lock = new object();

        // Job list
        internal List<Job> Jobs { get; } = new List<Job>();
        internal IEnumerable<CronJob> CronJobs => Jobs.OfType<CronJob>();
        internal IEnumerable<ServiceJob> ServiceJobs => Jobs.OfType<ServiceJob>();

        public CronManager(GlobalSettings settings, IEnumerable<IIntegration> integrations)
        {
            _integrations = integrations.ToArray();
            _settings = settings;

            DisplaySettingsInfo();
            Logger.Log($"{Jobs.Count} jobs in job list");

            RunBootJobs();

            if (_settings.RunMissedJobs)
                Logger.Catch(delegate
                {
                    var last = Registry.GetValue(RegKey, "LastRunTime", null) as string;

                    if (string.IsNullOrWhiteSpace(last) || !DateTime.TryParse(last, out var lastDt) || lastDt >= DateTime.Now)
                        return;

                    Logger.Debug("Run missed jobs mode - recalculating jobs execution time from last activity...");
                    foreach (var job in CronJobs)
                        job.RecalcNextExecTime(lastDt);
                });
        }

        public void DisplaySettingsInfo()
        {
            Logger.Log($"Config: Run missed jobs on startup: {_settings.RunMissedJobs}");
            Logger.Log($"Config: Check services every: {_settings.ServiceChecks} seconds" + (_settings.ServiceChecks == 0 ? " (constantly)" : ""));
        }

        public void Reload()
        {



            foreach (var integration in _integrations)
            {
                var jobsettings = new JobSettings
                {
                    Home = homedir
                };

                var result = integration.FetchJobs()
            }


            var newConfig = FileConfigReader.Load(ConfigFile);
            ConfigTime = ConfigFile.LastWriteTime;

            Config.Settings = newConfig.Settings;
            Config.DisplaySettingsInfo();
            
            // It's important to compare Config.Jobs first, since the "both" result will have items
            // from the first List - and the first list has all the Process identifiers, not newConfig.
            var result = CollectionExtensions.Intersect(Config.Jobs, newConfig.Jobs,
              (job1, job2) => string.Compare(job1.GetJobCode(), job2.GetJobCode(), StringComparison.InvariantCulture));

            Config.Jobs.Clear();

            // Add jobs that exist in both new and old
            Config.Jobs.AddRange(result.Both);

            // Add new jobs
            foreach (var job in result.Right)
            {
                Logger.Log("Found new " + job.GetType().Name + ": " + job.Command);
                Config.Jobs.Add(job);
            }

            // End service jobs no longer existing
            result.Left.ForEach(job => Logger.Log("Removing old " + job.GetType().Name + ": " + job.Command));
            TerminateJobs(result.Left.OfType<ServiceJob>());
        }

        public void RunBootJobs()
        {
            Logger.Debug("Starting boot jobs");
            foreach (var job in CronJobs.Where(job => job.Reboot))
                job.Run();
        }

        private void SaveDateTime()
        {
            Logger.Catch(() => Registry.SetValue(RegKey, "LastRunTime", DateTime.Now.ToString("s")));
        }

        public void Terminate()
        {
            TerminateJobs(ServiceJobs);
            SaveDateTime();
        }

        public static void TerminateJobs(IEnumerable<ServiceJob> services)
        {
            var options = new ParallelOptions { MaxDegreeOfParallelism = 10 };
            Parallel.ForEach(services, options, job =>
            {
                try
                {
                    job.Terminate();
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }
            });
        }

        public void Tick()
        {
            if (_inTick)
                return;

            lock (_lock)
            {
                if (_inTick)
                    return;

                _inTick = true;
                try
                {
                    Reload();

                    // Find any jobs that we've passed the Next Exec Time and run them.
                    var now = DateTime.Now;
                    foreach (var job in CronJobs.Where(job => job.NextExecTime <= now))
                        job.Run();

                    // Check on service jobs
                    if ((now - _lastService).TotalSeconds > _settings.ServiceChecks)
                    {
                        Logger.Debug("Checking services");
                        _lastService = now;
                        foreach (var job in ServiceJobs.Where(job => !job.CheckIsRunning()))
                            job.Run();
                    }

                    SaveDateTime();

                    if (_lastDate < DateTime.Today)
                    {
                        _lastDate = DateTime.Today;
                        Logger.Log($"Hello! I have {CronJobs.Count(x => x.NextExecTime.Date == _lastDate)} upcoming jobs today and {ServiceJobs.Count()} services running.");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("Exception in Tick(): " + ex.GetType().Name + ": " + ex.Message);
                }
                finally
                {
                    _inTick = false;
                }
            }
        }
    }
}
