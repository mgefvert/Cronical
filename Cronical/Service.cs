using System;
using System.ServiceProcess;
using System.Threading;
using DotNetCommons.Logging;

namespace Cronical
{
    public partial class Service : ServiceBase
    {
        protected Timer Timer;
        protected CronManager Manager;
        public string Filename { get; set; }

        public Service()
        {
            InitializeComponent();
        }

        public void Initialize()
        {
            Logger.Notice("Process startup");
            Logger.Log($"Using definition file {Filename}");

            Manager = new CronManager(Filename);
            Timer = new Timer(x => Manager.Tick(), null, 1000, 1000);
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
            Timer.Dispose();
            Manager.Terminate();
        }
    }
}
