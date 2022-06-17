using System.Reflection;
using Cronical.Configuration;
using Cronical.Misc;
using DotNetCommons.Sys;
using Serilog;

namespace Cronical;

internal static class Program
{
    private static CommandLineOptions _opts;
    public static IMailSender MailSender = new MailSender();

    /// <summary>
    /// Main program entry point; for when we're starting the program from the command line.
    /// Processes arguments, options, installs and removes the service.
    /// </summary>
    private static int Main(string[] args)
    {
        try
        {
            // Always reset the current working directory to where the executable is
            // Absolutely necessary for services, and probably the desired behavior for
            // console processes as well.
            var path = Path.GetDirectoryName(Path.GetFullPath(Environment.GetCommandLineArgs()[0]));
            if (!string.IsNullOrEmpty(path))
                Directory.SetCurrentDirectory(path);

            // Initialize the logging system
            var loggerConfig = new LoggerConfiguration();

            loggerConfig = _opts.DebugLogs ? loggerConfig.MinimumLevel.Debug() : loggerConfig.MinimumLevel.Information();
            loggerConfig = loggerConfig
                .WriteTo.File(path: Path.Combine(path, "cronical.log"), rollingInterval: RollingInterval.Month,
                    retainedFileCountLimit: 3)
                .WriteTo.Console();

            Log.Logger = loggerConfig.CreateLogger();

            // CTRL-C subprocess handler
            if ((args.FirstOrDefault() ?? "") == "ctrlc")
            {
                InjectCtrlC.Handle(args);
                return 0;
            }

            _opts = CommandLine.Parse<CommandLineOptions>(args);

            if (!File.Exists(_opts.ConfigFile))
                throw new FileNotFoundException("Can't find cron data file " + _opts.ConfigFile);

            Log.Information("Cronical booting up");

            var service = new Service(_opts.ConfigFile);

            if (_opts.InstallService)
            {
                // Install the program as a Windows service
                InstallService(_opts);
                return 0;
            }

            if (_opts.RemoveService)
            {
                // Remove the program from the list of Windows services
                RemoveService(_opts);
                return 0;
            }

            if (_opts.RunAsConsole)
            {
                Log.Information("Starting Cronical as a console program...");
                RunStandalone(service);
            }
            else
            {
                Log.Information("Starting Cronical as a service...");
                ServiceBase.Run(service);
            }

            return 0;
        }
        catch (OperationCanceledException)
        {
            // Thrown when we abort startup, perhaps to display help ... just exit
            return 1;
        }
        catch (Exception ex)
        {
            Log.Error(ex.GetType().Name + ": " + ex.Message);
            return 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    private static void InstallService(CommandLineOptions opts)
    {
        Log.Information($"Installing service '{opts.ServiceName}'...");

        var cmd = $"\"{Assembly.GetExecutingAssembly().Location}\"";
        if (opts.ConfigFileOverride)
        {
            var configFile = Path.GetFullPath(opts.ConfigFile);
            if (!File.Exists(configFile))
                throw new Exception($"Config file {configFile} does not exist!");

            cmd += $" -c \"{configFile}\"";
        }

        ServiceHelper.Install(opts.ServiceName, opts.ServiceTitle, cmd, ServiceHelper.ServiceBootFlag.AutoStart, opts.ServiceDescription);
    }

    private static void RemoveService(CommandLineOptions opts)
    {
        Log.Information($"Removing service '{opts.ServiceName}'...");
        ServiceHelper.Uninstall(opts.ServiceName);
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
                    Log.Information("Break signaled, exiting");
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