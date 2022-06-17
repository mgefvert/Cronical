using Cronical.Jobs;
using DotNetCommons;
using Serilog.Core;

namespace Cronical.JobRunners;

/// <summary>
/// Job class that handles a single job execution; once the job is run it is typically
/// discarded. Useful for implementing a job queue.
/// </summary>
public class ScheduledJobRunner : SingleJobRunner
{
    private DateTime? _nextRun;

    public ScheduledJobRunner(Logger log) : base(log)
    {
    }

    public override bool ShouldRun(Job job)
    {
        _nextRun ??= CalcNextExecTime(job);

        return _nextRun != null && DateTime.Now >= _nextRun;
    }

    public override void Run(Job job)
    {
        _nextRun = CalcNextExecTime(job);
        base.Run(job);
    }

    public DateTime? CalcNextExecTime(Job job)
    {
        // TODO: How to handle this
        if (job.Definition.OnReboot)
            return null;

        // Start at the current time plus one minute.
        var origin = DateTime.Now.Truncate(TimeSpan.TicksPerMinute);
        var end = origin.AddYears(1);

        // Loop through every single minute of the search space and test for execution.
        for (var test = origin.AddMinutes(1); test < end; test = test.AddMinutes(1))
        {
            if (job.Definition.Schedule!.Matches(test))
                return test;
        }

        // No time found
        return null;
    }
}