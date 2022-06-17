using System.Reflection;

namespace Cronical.Integrations;

/// <summary>
/// Settings for a particular job. Can be locked to prevent further changes (as in the
/// default job configuration loaded from registry) and copied to other jobs.
/// </summary>
public class JobSettingsBuilder
{
    public string? Home { get; set; }
    public bool MailStdOut { get; set; }
    public string? MailCc { get; set; }
    public string? MailBcc { get; set; }
    public string? MailFrom { get; set; }
    public string? MailTo { get; set; }
    public string? SmtpHost { get; set; }
    public string? SmtpPass { get; set; }
    public bool SmtpSSL { get; set; }
    public string? SmtpUser { get; set; }
    public int? Timeout { get; set; }

    public JobSettings Build()
    {
        return new JobSettings
        {
            Home = Home,
            MailStdOut = MailStdOut,
            MailCc = MailCc,
            MailBcc = MailBcc,
            MailFrom = MailFrom,
            MailTo = MailTo,
            SmtpHost = SmtpHost,
            SmtpPass = SmtpPass,
            SmtpSSL = SmtpSSL,
            SmtpUser = SmtpUser,
            Timeout = Timeout
        };
    }

    public override string ToString()
    {
        // Build a list of settings, but don't include global settings in job code comparisons
        var props = GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Select(p => (p.GetValue(this, null) ?? "").ToString())
            .ToList();

        return string.Join(",", props);
    }
}