using System;
using System.Linq;
using Cronical.Configuration;
using Cronical.Jobs;
using Cronical.Logging;
using Cronical.Misc;
using Microsoft.Win32;

namespace Cronical
{
  public class CronManager
  {
    public const string RegKey = @"HKEY_CURRENT_USER\Software\Ciceronen\Cronical";

    public string Filename { get; private set; }
    public Config Config { get; private set; }
    protected DateTime LastFileDate;
    protected int ServiceCounter;
    private DateTime _lastDate;
    private volatile bool _inTick;

    public CronManager(string filename)
    {
      Filename = filename;

      Config = new Config(Filename);
      LastFileDate = Config.FileDate;
      Logger.Log("{0} jobs in job list", Config.Jobs.Count);

      RunBootJobs();

      if (Config.Settings.RunMissedJobs)
        Logger.Catch(delegate
        {
          var last = Registry.GetValue(RegKey, "LastRunTime", null) as string;
          DateTime lastDt;

          if (string.IsNullOrWhiteSpace(last) || !DateTime.TryParse(last, out lastDt) || lastDt >= DateTime.Now)
            return;

          Logger.Debug("Run missed jobs mode - recalculating jobs execution time from last activity...");
          foreach (var job in Config.CronJobs)
            job.RecalcNextExecTime(lastDt);
        });
    }

    public bool HasConfigChanged()
    {
      return Config.FileDate > LastFileDate;
    }

    public void Reload()
    {
      var newConfig = new Config(Filename);
      LastFileDate = newConfig.FileDate;

      // It's important to compare Config.Jobs first, since the "both" result will have items
      // from the first List - and the first list has all the Process identifiers, not newConfig.
      var result = ListHelper.Intersect(Config.Jobs, newConfig.Jobs, 
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
      foreach (var job in result.Left)
      {
        Logger.Log("Removing old " + job.GetType().Name + ": " + job.Command);
        if (job is ServiceJob)
          ((ServiceJob)job).Terminate();
      }
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
      foreach (var job in Config.ServiceJobs)
        Logger.Catch(job.Terminate);

      SaveDateTime();
    }

    public void Tick()
    {
      if (_inTick)
        Logger.Log("Cannot enter Tick() - another thread already running");

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

        // Only check service jobs every minute
        if (ServiceCounter++%4 == 0)
        {
          foreach (var job in Config.ServiceJobs.Where(job => !job.IsRunning()))
            job.Run();
        }

        SaveDateTime();

        if (_lastDate < DateTime.Today)
        {
          _lastDate = DateTime.Today;
          Logger.Log("Tick: Hello! I have {0} upcoming jobs today and {1} services running.",
            Config.CronJobs.Count(x => x.NextExecTime.Date == _lastDate), Config.ServiceJobs.Count());
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
