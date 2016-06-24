using System;
using System.Linq;
using System.Reflection;
using Cronical.Misc;

namespace Cronical.Configuration
{
  public class CommandLineOptions
  {
    public CommandLineOptions(string[] args)
    {
      ConfigFile = "cronical.dat";
      ServiceName = "Cronical";
      ServiceTitle = "Cronical Job Scheduler";

      var arglist = args.ToList();
      while (arglist.Any())
      {
        var s = arglist.ExtractFirst().ToLower();

        switch (s)
        {
          case "-d":
          case "--debug":
            DebugLogs = true;
            break;

          case "--console":
            Console = true;
            break;

          case "-c":
          case "--config":
            ConfigFileOverride = true;
            ConfigFile = arglist.ExtractFirstOrDefault();
            if (string.IsNullOrEmpty(ConfigFile) || ConfigFile.StartsWith("-"))
              throw new Exception("Expected file name to follow on --config");
            break;

          case "-h":
          case "--help":
          case "-?":
          case "/h":
          case "/help":
            DisplayHelp();
            throw new OperationCanceledException();

          case "--install":
            InstallService = true;
            break;

          case "--remove":
            RemoveService = true;
            break;

          case "--service-name":
            ServiceName = arglist.ExtractFirstOrDefault();
            if (string.IsNullOrEmpty(ServiceName) || ServiceName.StartsWith("-"))
              throw new Exception("Expected name to follow after --service-name");
            break;

          case "--service-title":
            ServiceTitle = arglist.ExtractFirstOrDefault();
            if (string.IsNullOrEmpty(ServiceTitle) || ServiceTitle.StartsWith("-"))
              throw new Exception("Expected text to follow after --service-title");
            break;

          default:
            throw new Exception("Unrecognized option " + s);
        }
      }
    }

    public static void DisplayHelp()
    {
      System.Console.WriteLine("Cronical " + Assembly.GetExecutingAssembly().GetName().Version);
      System.Console.WriteLine();
      System.Console.WriteLine("Available options:");
      System.Console.WriteLine(" -d  --debug                 Use debug logging");
      System.Console.WriteLine("     --console               Run standalone using console for output");
      System.Console.WriteLine(" -c  --config <file>         Use alternate job file");
      System.Console.WriteLine(" -h  --help                  Display help");
      System.Console.WriteLine("     --install               Install as a service");
      System.Console.WriteLine("     --remove                Remove service installation");
      System.Console.WriteLine("     --service-name <name>   Force specific service name (default 'Cronical')");
      System.Console.WriteLine("     --service-title <text>  Set a different, readable title for the service");
      System.Console.WriteLine("                             (default is 'Cronical Job Scheduler')");
    }

    public bool Console { get; set; }
    public bool DebugLogs { get; set; }
    public string ConfigFile { get; set; }
    public bool ConfigFileOverride { get; set; }
    public bool InstallService { get; set; }
    public bool RemoveService { get; set; }
    public string ServiceName { get; set; }
    public string ServiceTitle { get; set; }
  }
}
