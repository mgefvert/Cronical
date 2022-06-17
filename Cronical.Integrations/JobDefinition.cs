namespace Cronical.Integrations;

public enum JobType
{
    SingleJob,
    ScheduledJob,
    ServiceJob,
    WatchJob
}

public class JobDefinition
{
    public JobType JobType { get; }

    public Schedule? Schedule { get; init; }

    public bool OnReboot { get; init; }

    public DirectoryInfo? Watch { get; init; }

    public JobSettings Settings { get; }

    public IIntegration Loader { get; }

    public string Command { get; }

    public string? Tag { get; init; }

    protected JobDefinition(JobType jobType, IIntegration loader, JobSettings settings, string command)
    {
        JobType = jobType;
        Loader = loader;
        Settings = settings;
        Command = command;
    }

    public static JobDefinition SingleJob(IIntegration loader, JobSettings settings, string command)
    {
        return new JobDefinition(JobType.SingleJob, loader, settings, command);
    }

    public static JobDefinition ScheduledJob(IIntegration loader, JobSettings settings, string command, Schedule schedule, bool onReboot)
    {
        return new JobDefinition(JobType.ScheduledJob, loader, settings, command)
        {
            OnReboot = onReboot,
            Schedule = schedule
        };
    }

    public static JobDefinition ServiceJob(IIntegration loader, JobSettings settings, string command)
    {
        return new JobDefinition(JobType.ServiceJob, loader, settings, command);
    }

    public static JobDefinition WatchJob(IIntegration loader, JobSettings settings, string command, DirectoryInfo watch)
    {
        return new JobDefinition(JobType.WatchJob, loader, settings, command)
        {
            Watch = watch
        };
    }
}