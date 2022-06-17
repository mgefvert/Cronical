using System.Reflection;

namespace Cronical.Integrations;

/// <summary>
/// Settings for a particular job. Can be locked to prevent further changes (as in the
/// default job configuration loaded from registry) and copied to other jobs.
/// </summary>
public class JobSettings
{
    public string? Home { get; init; }
    public bool MailStdOut { get; init; }
    public string? MailCc { get; init; }
    public string? MailBcc { get; init; }
    public string? MailFrom { get; init; }
    public string? MailTo { get; init; }
    public string? SmtpHost { get; init; }
    public string? SmtpPass { get; init; }
    public bool SmtpSSL { get; init; }
    public string? SmtpUser { get; init; }
    public int Timeout { get; init; }

    public override string ToString()
    {
        // Build a list of settings, but don't include global settings in job code comparisons
        var props = GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Select(p => (p.GetValue(this, null) ?? "").ToString())
            .ToList();

        return string.Join(",", props);
    }
}