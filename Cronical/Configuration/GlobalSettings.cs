using System;

namespace Cronical.Configuration
{
    public class GlobalSettings : AbstractSettings
    {
        public bool RunMissedJobs { get; set; }
        public int ServiceChecks { get; set; }
        public int Timeout { get; set; } = 86400;
    }
}
