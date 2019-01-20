using System;

namespace Cronical.Configuration
{
    public class GlobalSettings : AbstractSettings
    {
        public bool RunMissedJobs { get; set; }
        public int ServiceChecks { get; set; }
    }
}
