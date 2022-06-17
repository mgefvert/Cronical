namespace Cronical.Integrations;

/// <summary>
/// Global settings for the whole service.
/// </summary>
public class GlobalSettings
{
    public bool RunMissedJobs { get; set; }
    public int ServiceChecks { get; set; }
}