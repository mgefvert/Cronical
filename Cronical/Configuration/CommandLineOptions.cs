using System;
using System.Linq;
using Cronical.Misc;

namespace Cronical.Configuration
{
  public class CommandLineOptions
  {
    public CommandLineOptions(string[] args)
    {
      ConfigFile = "cronical.dat";

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
            ConfigFile = arglist.ExtractFirstOrDefault();
            if (ConfigFile == null)
              throw new Exception("Expected file name to follow on --config");
            break;

          case "--install":
            InstallService = true;
            break;

          case "--remove":
            RemoveService = true;
            break;

          default:
            throw new Exception("Unrecognized option " + s);
        }
      }
    }

    public bool Console { get; set; }
    public bool DebugLogs { get; set; }
    public string ConfigFile { get; set; }
    public bool InstallService { get; set; }
    public bool RemoveService { get; set; }
  }
}
