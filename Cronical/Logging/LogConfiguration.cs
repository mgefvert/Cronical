using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.Threading;

namespace Cronical.Logging
{
    public class LogConfiguration : ICloneable
    {
        public bool EchoToConsole { get; set; }
        public bool Loaded { get; set; }
        public int MainThreadId { get; set; }
        public string Path { get; set; }
        public string ProcessName { get; set; }
        public int Retention { get; set; }
        public LogSeverity Severity { get; set; }

        public LogConfiguration()
        {
            Reset();
        }

        public void Load()
        {
            InitializeFromConfiguration(ConfigurationManager.AppSettings);
            Loaded = true;
        }

        public void Reset()
        {
            EchoToConsole = false;
            MainThreadId = Thread.CurrentThread.ManagedThreadId;
            Path = System.IO.Directory.GetCurrentDirectory();
            ProcessName = System.IO.Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().ProcessName);
            Retention = 30;
            Severity = LogSeverity.Default;
        }

        internal void InitializeFromConfiguration(NameValueCollection config = null)
        {
            if (config != null)
            {
                string value;

                if (!string.IsNullOrWhiteSpace(value = config["LogEcho"]))
                    EchoToConsole = bool.Parse(value);

                if (!string.IsNullOrWhiteSpace(value = config["LogPath"]))
                    Path = value;

                if (!string.IsNullOrWhiteSpace(value = config["LogProcessName"]))
                    ProcessName = value;

                if (!string.IsNullOrWhiteSpace(value = config["LogRetention"]))
                    Retention = int.Parse(value);

                if (!string.IsNullOrWhiteSpace(value = config["LogDebug"]))
                    if (bool.Parse(value))
                        Severity = LogSeverity.Debug;
            }
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
