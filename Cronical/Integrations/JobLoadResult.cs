using System;

namespace Cronical.Integrations
{
    /// <summary>
    /// Enumeration of the possible job reload results that an integration can return.
    /// </summary>
    public enum JobLoadResult
    {
        /// <summary>
        /// No change. Maintain the current list of jobs.
        /// </summary>
        NoChange,

        /// <summary>
        /// Add the provided list of jobs to the active job list, keeping the existing
        /// jobs. Useful primarily if the integration has a job queue to process.
        /// </summary>
        AddJobs,

        /// <summary>
        /// Replace the active job list with a new one; the cron manager will check for
        /// running jobs and transfer the active state to the new list as needed. Any
        /// services no longer present will be terminated, and new services will be
        /// started.
        /// </summary>
        ReplaceJobs
    }
}
