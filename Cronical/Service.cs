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
        private readonly GlobalSettings _globalSettings = new GlobalSettings();
        private readonly JobSettings _defaultSettings = new JobSettings();
        private readonly string _configFilename;

        public Service(string configFilename)
        {
            InitializeComponent();
            _configFilename = configFilename;
        }

        public void Initialize()
        {
            Logger.Notice("Process startup");
            Logger.Log($"Using definition file {_configFilename}");

            _fileConfigReader = new FileConfigReader(_configFilename);
            _fileConfigReader.Initialize(_globalSettings, Logger.LogChannel);
            _integrations.Add(_fileConfigReader);

            foreach (var integration in ConfigurationManager.AppSettings["Integrations"].Split(',').TrimAndFilter())
                _integrations.AddIfNotNull(LoadIntegration(integration));

            _manager = new CronManager(_globalSettings, _defaultSettings, _integrations);
            _timer = new Timer(x => _manager.Tick(), null, 1000, 1000);
        }

        private IIntegration LoadIntegration(string integrationName)
        {
            try
            {
                var assembly = Assembly.Load(integrationName + ".dll");
                foreach (var type in assembly.GetTypes().Where(t => t.IsInstanceOfType(typeof(IIntegration))))
                {
                    var integration = (IIntegration)Activator.CreateInstance(type);
                    if (integration.Initialize())
                }
            }
            catch (Exception e)
            {
                Logger.Error($"Exception {e.GetType().Name} while loading integration '{integrationName}': {e.Message}");
                return null;
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
