using Cronical.Integrations;
using Cronical.JobRunners;
using Cronical.Jobs;
using DotNetCommons;
using Microsoft.Win32;
using Serilog;

namespace Cronical;

/// <summary>
/// CronManager manages the internal logic of the cron system. Maintains lists of jobs to be run,
/// active services, handles updating configuration and starts and monitors processes.
/// </summary>
public class CronManager
{
    public const string RegKey = @"HKEY_CURRENT_USER\Software\Ciceronen\Cronical";

    // Global configuration
    private readonly IIntegration[] _integrations;
    private readonly GlobalSettings _globalSettings;
    private readonly JobSettings _defaultSettings;

    // Timers and locks
    private DateTime _lastDate;
    private DateTime _lastService;
    private volatile bool _inTick;
    private readonly object _lock = new();

    // Job list
    internal List<Job> Jobs { get; } = new();
    internal IEnumerable<CronJob> CronJobs => Jobs.OfType<CronJob>();
    internal IEnumerable<ServiceJobRunner> ServiceJobs => Jobs.OfType<ServiceJobRunner>();

    /// <summary>
    /// Start the cron manager using a given configuration and applicable integrations.
    /// </summary>
    /// <param name="globalSettings">Global settings to use.</param>
    /// <param name="defaultSettings">Default job settings to use.</param>
    /// <param name="integrations">Loaded integrations.</param>
    public CronManager(GlobalSettings globalSettings, JobSettings defaultSettings, IEnumerable<IIntegration> integrations)
    {
        // Save configuration.
        _integrations = integrations.ToArray();
        _globalSettings = globalSettings;
        _defaultSettings = defaultSettings;

        DisplaySettingsInfo();
        Log.Information($"{Jobs.Count} jobs in job list");

        // Run any jobs scheduled at boot.
        RunBootJobs();

        // If configuration says to run missed jobs since last the service was
        // running, check those here.
        if (_globalSettings.RunMissedJobs)
        {
            try
            {
                var last = Registry.GetValue(RegKey, "LastRunTime", null) as string;

                if (string.IsNullOrWhiteSpace(last) || !DateTime.TryParse(last, out var lastDt) ||
                    lastDt >= DateTime.Now)
                    return;

                Log.Debug("Run missed jobs mode - recalculating jobs execution time from last activity...");
                foreach (var job in CronJobs)
                    job.RecalcNextExecTime(lastDt);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }
    }

    /// <summary>
    /// Dump global configuration information to screen.
    /// </summary>
    public void DisplaySettingsInfo()
    {
        Log.Information($"Config: Run missed jobs on startup: {_globalSettings.RunMissedJobs}");
        Log.Information($"Config: Check services every: {_globalSettings.ServiceChecks} seconds" + (_globalSettings.ServiceChecks == 0 ? " (constantly)" : ""));
    }

    /// <summary>
    /// Reload all jobs from all integrations (including the default file integration). Run fairly often,
    /// typically once per second; it is up to the various integrations to limit checking as needed
    /// to not overload the system (databases or similar).
    /// </summary>
    public void Reload()
    {
        foreach (var integration in _integrations)
            ReloadFromIntegration(integration);
    }

    /// <summary>
    /// Reload jobs from a given integration. The integration may specify a number of different
    /// actions; NoChange which means no change, AddJobs means it found additional jobs to add
    /// to the configuration, and Replace means to simply swap out the existing list for a new one.
    /// The integration may respond with different results at different times depending on the
    /// situation.
    /// </summary>
    /// <param name="integration">Integration to query.</param>
    public void ReloadFromIntegration(IIntegration integration)
    {
        var result = integration.FetchJobs(_defaultSettings);

        switch (result.Item1)
        {
            case JobLoadResult.NoChange:
                return;

            case JobLoadResult.AddJobs:
                Jobs.AddRange(result.Item2);
                return;

            case JobLoadResult.ReplaceJobs:
                // Continue below
                break;

            default:
                return;
        }

        // Replace jobs ... figure out which existing jobs belong to this integration and calculate
        // which ones to replace and which ones to keep.

        var myOldJobs = Jobs.ExtractAll(x => x.Loader == integration);

        // It's important to compare Config.Jobs first, since the "both" result will have items
        // from the first List - and the first list has all the Process identifiers, not newConfig.
        var comparison = myOldJobs.Intersect(result.Item2,
            (job1, job2) => string.Compare(job1.GetJobCode(), job2.GetJobCode(), StringComparison.InvariantCulture));

        // Add jobs that exist in both new and old
        Jobs.AddRange(comparison.Both);

        // Add new jobs
        foreach (var job in comparison.Right)
        {
            Log.Information("Found new " + job.GetType().Name + ": " + job.Command);
            Jobs.Add(job);
        }

        // End service jobs no longer existing
        comparison.Left.ForEach(job => Log.Information("Removing old " + job.GetType().Name + ": " + job.Command));
        TerminateJobs(comparison.Left.OfType<ServiceJobRunner>());
    }

    /// <summary>
    /// Run all jobs marked as @reboot.
    /// </summary>
    public void RunBootJobs()
    {
        Log.Debug("Starting boot jobs");
        foreach (var job in CronJobs.Where(job => job.Reboot))
            job.Run();
    }

    /// <summary>
    /// Save the last run time to registry.
    /// </summary>
    private void SaveDateTime()
    {
        try
        {
            Registry.SetValue(RegKey, "LastRunTime", DateTime.Now.ToString("s"));
        }
        catch (Exception e)
        {
            Log.Error(e.Message);
        }
    }

    /// <summary>
    /// Shut down the cron manager.
    /// </summary>
    public void Terminate()
    {
        TerminateJobs(ServiceJobs);
        SaveDateTime();
    }

    /// <summary>
    /// Terminate a list of services; will operate in parallel to make things
    /// quicker.
    /// </summary>
    /// <param name="services"></param>
    public static void TerminateJobs(IEnumerable<ServiceJobRunner> services)
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
                Log.Error(e.Message);
            }
        });
    }

    /// <summary>
    /// The Tick() method performs all the logic to monitor processes, see if new
    /// ones should be started, and perform minor housekeeping. Typically run once
    /// per second.
    /// </summary>
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
                // Check for new jobs
                Reload();

                // Find any jobs that we've passed the Next Exec Time and run them.
                var now = DateTime.Now;
                foreach (var job in CronJobs.Where(job => job.NextExecTime <= now))
                    job.Run();

                // Check on service jobs and verify that they're running
                if ((now - _lastService).TotalSeconds > _globalSettings.ServiceChecks)
                {
                    Log.Debug("Checking services");
                    _lastService = now;
                    foreach (var job in ServiceJobs.Where(job => !job.UpdateState()))
                        job.Run();
                }

                // Run single jobs and discard them
                foreach (var job in Jobs.ExtractAll(x => x is SingleJobRunner).Cast<SingleJobRunner>())
                    job.Run();

                SaveDateTime();

                // Did we roll over to a new day?
                if (_lastDate < DateTime.Today)
                {
                    _lastDate = DateTime.Today;
                    Log.Information($"Hello! I have {CronJobs.Count(x => x.NextExecTime.Date == _lastDate)} upcoming jobs today and {ServiceJobs.Count()} services running.");
                }
            }
            catch (Exception ex)
            {
                Log.Error("Exception in Tick(): " + ex.GetType().Name + ": " + ex.Message);
            }
            finally
            {
                _inTick = false;
            }
        }
    }
}