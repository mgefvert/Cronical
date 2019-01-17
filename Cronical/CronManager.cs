using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Cronical.Configuration;
using Cronical.Jobs;
using DotNetCommons.Collections;
using DotNetCommons.Logging;
using Microsoft.Win32;

namespace Cronical
{
    public class CronManager
    {
        public const string RegKey = @"HKEY_CURRENT_USER\Software\Ciceronen\Cronical";

        public FileInfo ConfigFile { get; }
        public Config Config { get; }
        protected DateTime ConfigTime;
        private DateTime _lastDate;
        private DateTime _lastService;
        private volatile bool _inTick;
        private readonly object _lock = new object();

        public CronManager(string filename)
        {
            ConfigFile = new FileInfo(filename);
            ConfigTime = ConfigFile.LastWriteTime;

            Config = ConfigReader.Load(ConfigFile);
            Config.DisplaySettingsInfo();

            Logger.Log($"{Config.Jobs.Count} jobs in job list");

            RunBootJobs();

            if (Config.Settings.RunMissedJobs)
                Logger.Catch(delegate
                {
                    var last = Registry.GetValue(RegKey, "LastRunTime", null) as string;

                    if (string.IsNullOrWhiteSpace(last) || !DateTime.TryParse(last, out var lastDt) || lastDt >= DateTime.Now)
                        return;

                    Logger.Debug("Run missed jobs mode - recalculating jobs execution time from last activity...");
                    foreach (var job in Config.CronJobs)
                        job.RecalcNextExecTime(lastDt);
                });
        }

        public bool HasConfigChanged()
        {
            ConfigFile.Refresh();
            return ConfigFile.LastWriteTime > ConfigTime;
        }

        public void Reload()
        {
            var newConfig = ConfigReader.Load(ConfigFile);
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
            foreach (var job in Config.CronJobs.Where(job => job.Reboot))
                job.Run();
        }

        private void SaveDateTime()
        {
            Logger.Catch(() => Registry.SetValue(RegKey, "LastRunTime", DateTime.Now.ToString("s")));
        }

        public void Terminate()
        {
            TerminateJobs(Config.ServiceJobs);
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
                    if (HasConfigChanged())
                    {
                        Logger.Log("Definition file change detected");
                        Reload();
                        Logger.Log("All jobs reloaded");
                    }

                    // Find any jobs that we've passed the Next Exec Time and run them.
                    var now = DateTime.Now;
                    foreach (var job in Config.CronJobs.Where(job => job.NextExecTime <= now))
                        job.Run();

                    // Check on service jobs
                    if ((now - _lastService).TotalSeconds > Config.Settings.ServiceChecks)
                    {
                        Logger.Debug("Checking services");
                        _lastService = now;
                        foreach (var job in Config.ServiceJobs.Where(job => !job.CheckIsRunning()))
                            job.Run();
                    }

                    SaveDateTime();

                    if (_lastDate < DateTime.Today)
                    {
                        _lastDate = DateTime.Today;
                        Logger.Log($"Hello! I have {Config.CronJobs.Count(x => x.NextExecTime.Date == _lastDate)} upcoming jobs today and {Config.ServiceJobs.Count()} services running.");
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
