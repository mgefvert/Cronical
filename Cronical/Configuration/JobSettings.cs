using System;
using System.Linq;
using System.Reflection;

namespace Cronical.Configuration
{
    public class JobSettings : AbstractSettings
    {
        public string Home { get; set; }
        public bool MailStdOut { get; set; }
        public string MailCc { get; set; }
        public string MailBcc { get; set; }
        public string MailFrom { get; set; }
        public string MailTo { get; set; }
        public string SmtpHost { get; set; }
        public string SmtpPass { get; set; }
        public bool SmtpSSL { get; set; }
        public string SmtpUser { get; set; }

        public JobSettings Clone()
        {
            return (JobSettings)MemberwiseClone();
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
}
