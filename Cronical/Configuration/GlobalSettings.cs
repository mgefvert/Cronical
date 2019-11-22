using System;

namespace Cronical.Configuration
{
    /// <summary>
    /// Global settings for the whole service.
    /// </summary>
    public class GlobalSettings : AbstractSettings
    {
        public bool RunMissedJobs { get; set; }
        public int ServiceChecks { get; set; }
    }
}
