using Cronical.Integrations;
using Cronical.Jobs;
using Cronical.Misc;
using Serilog.Core;

namespace Cronical.JobRunners;

/// <summary>
/// Job class that handles a single job execution; once the job is run it is typically
/// discarded. Useful for implementing a job queue.
/// </summary>
public class SingleJobRunner : IJobRunner
{
    protected readonly Logger Log;
    protected bool IsRunning;

    public SingleJobRunner(Logger log)
    {
        Log = log;
    }

    public virtual bool ShouldRun(Job job)
    {
        return job.Result == null;
    }

    public virtual void Run(Job job)
    {
        job.Result = new JobResult();
        new Thread(() => RunJobThread(job)).Start();
    }

    public string RunJobThread(Job job)
    {
        IsRunning = true;
        string? output = null;
        try
        {
            var process = new ProcessWrapper(job.Command, job.Settings.Home, true, job.Settings.MailStdOut);

            Log.Information("Starting job: " + job.Command);
            process.Start();

            Log.Debug($"Process started, waiting at most {job.Definition.Settings.Timeout} seconds");
            process.WaitForEnd(job.Settings.Timeout * 1000);
            output = process.FetchResult();

            Helper.SendMail("Cronical: Results from " + job.Command, output, job.Settings);
        }
        catch (Exception e)
        {
            var text = $"Failed to start process '{job.Command}': {e.Message}";
            Log.Error(text);
            Helper.SendMail("Cronical: Failed to start " + job.Command, text, job.Settings);
        }
        finally
        {
            IsRunning = false;
        }

        Log.Debug("Job finished");
        return output;
    }
}