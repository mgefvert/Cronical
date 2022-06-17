using Cronical.Jobs;
using Cronical.Misc;
using Serilog.Core;

namespace Cronical.JobRunners;

/// <summary>
/// Job class that handles and monitors services; starting them, watching over
/// them to make sure they're still running, terminating and restarting them
/// as needed.
/// </summary>
public class ServiceJobRunner : IJobRunner
{
    private readonly Logger _log;
    protected ProcessWrapper? Process;

    public JobState State { get; private set; }

    public ServiceJobRunner(Logger log)
    {
        _log = log;
    }

    public bool ShouldRun(Job job)
    {
        UpdateState(job);
        return State == JobState.Inactive;
    }

    public void Run(Job job)
    {
        UpdateState(job);
        switch (State)
        {
            case JobState.Starting:
                _log.Warning($"Run: Service is already starting: '{job.Command}'");
                return;

            case JobState.Running:
                _log.Warning($"Run: Job is already running: '{job.Command}'");
                return;

            case JobState.Stopping:
                _log.Warning($"Run: Unable to start, job is stopping: '{job.Command}'");
                return;
        }

        State = JobState.Starting;
        Process = new ProcessWrapper(job.Command, job.Settings.Home, false, false);

        try
        {
            _log.Information("Starting service: " + job.Command);
            Process.Start();
            _log.Debug("Process started");
        }
        catch (Exception e)
        {
            var text = $"Failed to start service '{job.Command}': {e.Message}";
            _log.Error(text);
            Helper.SendMail("Cronical: Failed to start service " + job.Command, text, job.Settings);
        }
        finally
        {
            State = UpdateState(job) ? JobState.Running : JobState.Inactive;
        }
    }

    public bool UpdateState(Job job)
    {
        var result = Process != null && Process.Running;

        if (State == JobState.Running && result == false)
        {
            _log.Warning($"Service terminated unexpectedly: '{job.Command}'");
            State = JobState.Inactive;
        }

        return result;
    }

    public void Terminate(Job job)
    {
        UpdateState(job);

        switch (State)
        {
            case JobState.Starting:
            case JobState.Running:
                _log.Information("Terminating service: " + job.Command);
                Process.Stop();
                return;

            case JobState.Stopping:
                _log.Warning($"Run: Job is already stopping: '{job.Command}'");
                return;
        }
    }
}