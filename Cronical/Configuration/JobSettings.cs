using System;
using System.Linq;
using System.Reflection;

namespace Cronical.Configuration
{
    public class JobSettings : AbstractSettings
    {
        private bool _locked;
        private string _home;
        private bool _mailStdOut;
        private string _mailCc;
        private string _mailBcc;
        private string _mailFrom;
        private string _mailTo;
        private string _smtpHost;
        private string _smtpPass;
        private bool _smtpSSL;
        private string _smtpUser;
        private int _timeout = 86400;

        public string Home
        {
            get => _home;
            set => _home = !_locked ? value : throw AccessDenied();
        }

        public bool MailStdOut
        {
            get => _mailStdOut;
            set => _mailStdOut = !_locked ? value : throw AccessDenied();
        }

        public string MailCc
        {
            get => _mailCc;
            set => _mailCc = !_locked ? value : throw AccessDenied();
        }

        public string MailBcc
        {
            get => _mailBcc;
            set => _mailBcc = !_locked ? value : throw AccessDenied();
        }

        public string MailFrom
        {
            get => _mailFrom;
            set => _mailFrom = !_locked ? value : throw AccessDenied();
        }

        public string MailTo
        {
            get => _mailTo;
            set => _mailTo = !_locked ? value : throw AccessDenied();
        }

        public string SmtpHost
        {
            get => _smtpHost;
            set => _smtpHost = !_locked ? value : throw AccessDenied();
        }

        public string SmtpPass
        {
            get => _smtpPass;
            set => _smtpPass = !_locked ? value : throw AccessDenied();
        }

        public bool SmtpSSL
        {
            get => _smtpSSL;
            set => _smtpSSL = !_locked ? value : throw AccessDenied();
        }

        public string SmtpUser
        {
            get => _smtpUser;
            set => _smtpUser = !_locked ? value : throw AccessDenied();
        }

        public int Timeout
        {
            get => _timeout;
            set => _timeout = !_locked ? value : throw AccessDenied();
        }

        private Exception AccessDenied()
        {
            return new Exception("Access denied");
        }

        public JobSettings Clone()
        {
            var result = (JobSettings)MemberwiseClone();
            result._locked = false;
            return result;
        }

        public void Lock()
        {
            _locked = true;
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
