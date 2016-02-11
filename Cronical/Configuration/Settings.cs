using System;
using System.Linq;
using System.Reflection;
using Cronical.Logging;

namespace Cronical.Configuration
{
  public class Settings
  {
    public string Home { get; set; }
    public bool MailStdOut { get; set; }
    public string MailCc { get; set; }
    public string MailBcc { get; set; }
    public string MailFrom { get; set; }
    public string MailTo { get; set; }
    public bool RunMissedJobs { get; set; }
    public string SmtpHost { get; set; }
    public string SmtpPass { get; set; }
    public bool SmtpSSL { get; set; }
    public string SmtpUser { get; set; }
    public int Timeout { get; set; }

    public Settings()
    {
      Timeout = 86400;
    }

    public Settings Clone()
    {
      return (Settings)MemberwiseClone();
    }

    public bool Exists(string setting)
    {
      return GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
        .Any(p => p.Name.Equals(setting, StringComparison.InvariantCultureIgnoreCase));
    }

    public void Set(string setting, string value)
    {
      var prop = GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
        .Single(p => p.Name.Equals(setting, StringComparison.InvariantCultureIgnoreCase));

      if (prop.PropertyType == typeof(bool))
      {
        bool val;
        if (bool.TryParse(value, out val))
          prop.SetValue(this, val, null);
        else
          Logger.Error("Value '{0}' is not recognized as a boolean value", value);
      }
      else if (prop.PropertyType == typeof(int))
      {
        int val;
        if (int.TryParse(value, out val))
          prop.SetValue(this, val, null);
        else
          Logger.Error("Value '{0}' is not recognized as an integer value", value);
      }
      else
        prop.SetValue(this, value, null);
    }

    public override string ToString()
    {
      return string.Join(",", GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
        .Select(p => (p.GetValue(this, null) ?? "").ToString()));
    }
  }
}
