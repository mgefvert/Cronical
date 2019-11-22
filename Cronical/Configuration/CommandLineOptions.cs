using System;
using System.Reflection;
using DotNetCommons.Sys;

// ReSharper disable LocalizableElement

namespace Cronical.Configuration
{
    /// <summary>
    /// Handles configuration given on the command line.
    /// </summary>
    public class CommandLineOptions
    {
        private string _configFile;
        private bool _configFileOverride;

        public CommandLineOptions()
        {
            _configFile = "cronical.dat";
            ServiceName = "Cronical";
            ServiceTitle = "Cronical Job Scheduler";
        }

        public static void DisplayHelp()
        {
            Console.WriteLine("Cronical " + Assembly.GetExecutingAssembly().GetName().Version);
            Console.WriteLine();
            Console.WriteLine("Available options:");
            Console.WriteLine(CommandLine.GetFormattedHelpText(typeof(CommandLineOptions)));
        }

        [CommandLineOption('c', "config", "Use alternate job file")]
        public string ConfigFile
        {
            get => _configFile;
            set
            {
                _configFile = value;
                _configFileOverride = true;
            }
        }

        public bool ConfigFileOverride => _configFileOverride;

        [CommandLineOption('d', "debug", "Use debug logging")]
        public bool DebugLogs { get; set; }

        [CommandLineOption(new[] { 'h', '?' }, new[] { "help" }, "Display help")]
        public bool Help { get; set; }

        [CommandLineOption("install", "Install as a service")]
        public bool InstallService { get; set; }

        [CommandLineOption("remove", "Remove service installation")]
        public bool RemoveService { get; set; }

        [CommandLineOption("console", "Run standalone using console for output")]
        public bool RunAsConsole { get; set; }

        [CommandLineOption("service-name", "Force specific service name (default='Cronical')")]
        public string ServiceName { get; set; }

        [CommandLineOption("service-title", "Set a different readable title (default='Cronical Job Scheduler')")]
        public string ServiceTitle { get; set; }

        [CommandLineOption("service-desc", "Set a description for the service")]
        public string ServiceDescription { get; set; }
    }
}
