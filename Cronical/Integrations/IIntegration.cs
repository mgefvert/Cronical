using System;
using System.Collections.Generic;
using Cronical.Configuration;
using Cronical.Jobs;
using DotNetCommons.Logging;

namespace Cronical.Integrations
{
    /// <summary>
    /// Interface implemented for all integrations.
    /// </summary>
    public interface IIntegration
    {
        /// <summary>
        /// Initialize the integration. Set up environment, start database connections,
        /// whatever is necessary.
        /// </summary>
        /// <param name="settings">A reference to the global settings.</param>
        /// <param name="logger">A Logger object to use for logging.</param>
        /// <returns>True if the integration is valid and should be added to CronManagers list of integrations. False
        ///     to disable the integration.</returns>
        bool Initialize(GlobalSettings settings, LogChannel logger);

        /// <summary>
        /// Reload a list of jobs from an external source. A copy of the default settings
        /// is provided which can be copied to the new jobs in the returned result.
        /// FetchJobs() is run fairly often, up to once per second, and it is the responsibility
        /// of the integration to keep track of how often the configuration should actually be
        /// checked. If the integration doesn't want to check this time, NoChange can simply be
        /// returned and no change will be made.
        /// </summary>
        /// <param name="defaultSettings">A reference to the default job settings.</param>
        /// <returns>A tuple containing the result of the operation, as well as a list of
        ///     jobs.</returns>
        (JobLoadResult, List<Job>) FetchJobs(JobSettings defaultSettings);

        /// <summary>
        /// Notification that a job completed. The integration may choose to register the result of the
        /// job or discard it.
        /// </summary>
        /// <param name="job">Job that was completed.</param>
        void Completed(Job job);

        /// <summary>
        /// Shut down the integration; the service is terminating.
        /// </summary>
        void Shutdown();
    }
}
