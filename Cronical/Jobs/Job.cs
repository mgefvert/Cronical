using Cronical.Integrations;
using Cronical.JobRunners;
using Cronical.Misc;
using Serilog;
using Serilog.Core;

namespace Cronical.Jobs;

/// <summary>
/// Abstract base class for all jobs. Maintains which integration loaded the job,
/// settings, etc.
/// </summary>
public class Job
{
    public JobDefinition Definition { get; set; }
    public IJobRunner JobRunner { get; set; }
    public JobResult? Result { get; set; }

    public string Command => Definition.Command;
    public JobSettings Settings => Definition.Settings;

    public Job(JobDefinition definition, Logger log)
    {
        Definition = definition;
        JobRunner = definition.JobType switch
        {
            JobType.SingleJob => new SingleJobRunner(log),
            JobType.ScheduledJob => new ScheduledJobRunner(log),
            JobType.ServiceJob => new ServiceJobRunner(log),
            JobType.WatchJob => new WatchJobRunner(log),
            _ => throw new ArgumentException("JobType is not valid")
        };
    }

    public string GetJobCode()
    {
        return GetType().Name + "," + Definition.Command + "," + Definition.Settings;
    }

    public void VerifyExecutableExists()
    {
        var process = new ProcessParameters(Definition.Command, Definition.Settings.Home);
        if (!File.Exists(process.Executable))
            Log.Warning($"File {process.Executable} does not seem to exist");
    }
}