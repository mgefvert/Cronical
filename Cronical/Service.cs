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
    /// <summary>
    /// The Service class is the fundamental crontroller of Cronical, responsible
    /// for instantiating and maintaining all the different components required.
    /// It can be run either as a service, or standalone as a console.
    /// </summary>
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

        /// <summary>
        /// Initialize the service and configure it; along with loading all the integrations
        /// specified in the app.config file.
        /// </summary>
        public void Initialize()
        {
            Logger.Notice("Process startup");
            Logger.Log($"Using definition file {_configFilename}");

            // One FileConfigReader is always required
            _fileConfigReader = new FileConfigReader(_configFilename);
            _fileConfigReader.Initialize(_globalSettings, Logger.LogChannel);
            _integrations.Add(_fileConfigReader);

            // Create additional integrations
            foreach (var integration in ConfigurationManager.AppSettings["Integrations"].Split(',').TrimAndFilter())
                _integrations.AddRangeIfNotNull(LoadIntegration(integration));

            // Start cron manager and start ticking
            _manager = new CronManager(_globalSettings, _defaultSettings, _integrations);
            _timer = new Timer(x => _manager.Tick(), null, 1000, 1000);
        }

        /// <summary>
        /// Load default settings from the app.config file.
        /// </summary>
        /// <returns></returns>
        public static (GlobalSettings, JobSettings) LoadDefaultSettings()
        {
            // Global settings are global for entire service.
            var globalSettings = new GlobalSettings
            {
                RunMissedJobs = ConfigurationManager.AppSettings["RunMissedJobs"].ParseBoolean(),
                ServiceChecks = ConfigurationManager.AppSettings["ServiceChecks"].ParseInt()
            };

            // Default job settings are given in the app.config file and can be overriden by
            // integrations.
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

        /// <summary>
        /// Load an integration and initialize it.
        /// </summary>
        /// <param name="integrationName">Integration name without the .DLL part.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Service start command (for Windows services)
        /// </summary>
        /// <param name="args"></param>
        protected override void OnStart(string[] args)
        {
            Console.CancelKeyPress += (sender, breakArgs) => breakArgs.Cancel = true;
            Initialize();
        }

        /// <summary>
        /// Service stop command (for Windows services)
        /// </summary>
        protected override void OnStop()
        {
            Shutdown();
        }

        /// <summary>
        /// Shut down the service and terminate.
        /// </summary>
        public void Shutdown()
        {
            Logger.Notice("Shutting down");
            _timer.Dispose();
            _manager.Terminate();
        }
    }
}
