using Cronical.Jobs;

namespace Cronical.JobRunners;

public interface IJobRunner
{
    bool ShouldRun(Job job);
    void Run(Job job);
}