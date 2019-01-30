using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Threading;
using Cronical.Configuration;
using Cronical.Integrations;
using DotNetCommons.Collections;
using DotNetCommons.Logging;
using DotNetCommons.Text;

namespace Cronical
{
    public partial class Service : ServiceBase
    {
        private Timer _timer;
        private CronManager _manager;
        private FileConfigReader _fileConfigReader;

        private readonly List<IIntegration> _integrations = new List<IIntegration>();
        private readonly GlobalSettings _globalSettings;
        private readonly JobSettings _defaultSettings;
        private readonly string _configFilename;

        public Service(string configFilename)
        {
            InitializeComponent();
            _configFilename = configFilename;
            (_globalSettings, _defaultSettings) = LoadDefaultSettings();
        }

        public void Initialize()
        {
            Logger.Notice("Process startup");
            Logger.Log($"Using definition file {_configFilename}");

            _fileConfigReader = new FileConfigReader(_configFilename);
            _fileConfigReader.Initialize(_globalSettings, Logger.LogChannel);
            _integrations.Add(_fileConfigReader);

            foreach (var integration in ConfigurationManager.AppSettings["Integrations"].Split(',').TrimAndFilter())
                _integrations.AddRangeIfNotNull(LoadIntegration(integration));

            _manager = new CronManager(_globalSettings, _defaultSettings, _integrations);
            _timer = new Timer(x => _manager.Tick(), null, 1000, 1000);
        }

        public static (GlobalSettings, JobSettings) LoadDefaultSettings()
        {
            var globalSettings = new GlobalSettings
            {
                RunMissedJobs = ConfigurationManager.AppSettings["RunMissedJobs"].ParseBoolean(),
                ServiceChecks = ConfigurationManager.AppSettings["ServiceChecks"].ParseInt()
            };

            var defaultSettings = new JobSettings
            {
                Home       = Path.GetFullPath(ConfigurationManager.AppSettings["Home"] ?? "."),
                MailStdOut = ConfigurationManager.AppSettings["MailStdOut"].ParseBoolean(),
                MailCc     = ConfigurationManager.AppSettings["MailCc"],
                MailBcc    = ConfigurationManager.AppSettings["MailBcc"],
                MailFrom   = ConfigurationManager.AppSettings["MailFrom"],
                MailTo     = ConfigurationManager.AppSettings["MailTo"],
                SmtpHost   = ConfigurationManager.AppSettings["SmtpHost"],
                SmtpPass   = ConfigurationManager.AppSettings["SmtpPass"],
                SmtpSSL    = ConfigurationManager.AppSettings["SmtpSSL"].ParseBoolean(),
                SmtpUser   = ConfigurationManager.AppSettings["SmtpUser"],
                Timeout    = ConfigurationManager.AppSettings["Timeout"].ParseInt(86400)
            };
            defaultSettings.Lock();

            return (globalSettings, defaultSettings);
        }


        private IEnumerable<IIntegration> LoadIntegration(string integrationName)
        {
            var assembly = Assembly.Load(integrationName + ".dll");
            foreach (var type in assembly.GetTypes().Where(t => t.IsInstanceOfType(typeof(IIntegration))))
            {
                IIntegration integration = null;
                try
                {
                    integration = (IIntegration)Activator.CreateInstance(type);
                }
                catch (Exception e)
                {
                    Logger.Error($"Exception {e.GetType().Name} while loading integration '{integrationName}': {e.Message}");
                }

                if (integration != null && integration.Initialize(_globalSettings, Logger.LogChannel))
                    yield return integration;
            }
        }

        protected override void OnStart(string[] args)
        {
            Console.CancelKeyPress += (sender, breakArgs) => breakArgs.Cancel = true;
            Initialize();
        }

        protected override void OnStop()
        {
            Shutdown();
        }

        public void Shutdown()
        {
            Logger.Notice("Shutting down");
            _timer.Dispose();
            _manager.Terminate();
        }
    }
}
