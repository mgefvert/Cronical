using System;
using System.IO;
using System.Reflection;
using System.ServiceProcess;
using System.Threading;
using Cronical.Configuration;
using Cronical.Logging;
using Cronical.Misc;

namespace Cronical
{
  internal static class Program
  {
    private static CommandLineOptions _opts;
    public static IMailSender MailSender = new MailSender();

    static void Main(string[] args)
    {
      try
      {
        _opts = new CommandLineOptions(args);

        // Always reset the current working directory to where the executable is
        // Absolutely necessary for services, and probably the desired behavior for
        // console processes as well.
        var path = Path.GetDirectoryName(Path.GetFullPath(Environment.GetCommandLineArgs()[0]));
        if (!string.IsNullOrEmpty(path))
          Directory.SetCurrentDirectory(path);

        // Initialize the logging system
        Logger.Configuration.EchoToConsole = true;
        Logger.Configuration.Path = path;
        Logger.Configuration.ProcessName = "cronical";
        Logger.Configuration.Retention = 3;

        // Load the configuration file and scan it for global keywords
        InitialConfigScan();
        Logger.Notice("Cronical booting up");

        var service = new Service { Filename = _opts.ConfigFile };

        if (_opts.InstallService)
        {
          // Install the program as a Windows service
          InstallService();
          return;
        }

        if (_opts.RemoveService)
        {
          // Remove the program from the list of Windows services
          RemoveService();
          return;
        }

        if (_opts.Console)
        {
          Logger.Log("Starting Cronical as a console program...");
          RunStandalone(service);
        }
        else
        {
          Logger.Log("Starting Cronical as a service...");
          ServiceBase.Run(service);
        }
      }
      catch (Exception ex)
      {
        Logger.Error(ex.GetType().Name + ": " + ex.Message);
      }
    }

    private static void InitialConfigScan()
    {
      if (!File.Exists(_opts.ConfigFile))
        throw new FileNotFoundException("Can't find cron data file " + _opts.ConfigFile);

      string logpath = null;
      var reader = new ConfigReader();
      reader.DefinitionRead += (sender, args) =>
      {
        if (args.Definition.Equals("LOGPATH", StringComparison.InvariantCultureIgnoreCase))
          logpath = args.Value;
      };

      reader.Read(_opts.ConfigFile);

      // If we found a LogPath command, reinitialize the logger
      if (logpath != null)
        Logger.Configuration.Path = logpath;

      Logger.Configuration.Severity = _opts.DebugLogs ? LogSeverity.Debug : LogSeverity.Default;
    }

    private static void InstallService()
    {
      Logger.Log("Installing service...");
      ServiceHelper.Install("Cronical", "Ciceronen Cronical", Assembly.GetExecutingAssembly().Location);
    }

    private static void RemoveService()
    {
      Logger.Log("Removing service...");
      ServiceHelper.Uninstall("Cronical");
    }

    private static void RunStandalone(Service service)
    {
      var ctrlCFired = false;

      service.Initialize();
      try
      {
        var breakEvent = new ManualResetEvent(false);

        Console.CancelKeyPress += (sender, args) =>
        {
          args.Cancel = true;
          breakEvent.Set();

          if (ctrlCFired == false)
          {
            Logger.Notice("Break signaled, exiting");
            ctrlCFired = true;
          }
        };

        // Run until CTRL-C or CTRL-BREAK
        breakEvent.WaitOne();
      }
      finally
      {
        service.Shutdown();
      }
    }
  }
}
