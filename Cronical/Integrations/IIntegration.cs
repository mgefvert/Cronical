using System;
using System.Collections.Generic;
using Cronical.Configuration;
using Cronical.Jobs;
using DotNetCommons.Logging;

namespace Cronical.Integrations
{
    public interface IIntegration
    {
        bool Initialize(GlobalSettings settings, LogChannel logger);
        (JobLoadResult, List<Job>) FetchJobs(JobSettings defaultSettings);
        void Completed(Job job);
        void Shutdown();
    }
}
