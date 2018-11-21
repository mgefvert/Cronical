using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cronical.Jobs;
using Cronical.Logging;

namespace Cronical.Configuration
{
    public class Config
    {
        public List<Job> Jobs = new List<Job>();
        public Settings Settings = new Settings();
        protected ConfigReader Reader;

        public DirectoryInfo Path { get; }
        public FileInfo File { get; }

        public DateTime FileDate
        {
            get
            {
                File.Refresh();
                return File.LastWriteTime;
            }
        }

        public IEnumerable<CronJob> CronJobs => Jobs.OfType<CronJob>();
        public IEnumerable<ServiceJob> ServiceJobs => Jobs.OfType<ServiceJob>();

        public Config(string filename)
        {
            File = new FileInfo(filename);
            Path = File.Directory ?? new DirectoryInfo(Directory.GetCurrentDirectory());

            Reader = new ConfigReader();
            Reader.DefinitionRead += ConfigReaderOnDefinitionRead;
            Reader.JobRead += ConfigReaderOnJobRead;
            Reader.InvalidConfig += ConfigReaderOnInvalidConfig;

            Reload();
        }

        public void Reload()
        {
            Reader.Read(File.FullName);
        }

        private void ConfigReaderOnDefinitionRead(object sender, ConfigReader.DefinitionArgs args)
        {
            // This is handled another way..
            if (args.Definition.Equals("LogPath", StringComparison.InvariantCultureIgnoreCase))
                return;

            if (Settings.Exists(args.Definition))
                Settings.Set(args.Definition, args.Value);
            else
                Logger.Error("Invalid definition: {0}", args.Definition);
        }

        private void ConfigReaderOnInvalidConfig(object sender, ConfigReader.InvalidConfigArgs args)
        {
            Logger.Error("Invalid definition on line {0}: {1}", args.LineNo, args.Text);
        }

        private void ConfigReaderOnJobRead(object sender, ConfigReader.JobArgs jobArgs)
        {
            var job = jobArgs.Service ? (Job)ServiceJob.Parse(jobArgs, Settings) : CronJob.Parse(jobArgs, Settings);
            if (job == null)
                return;

            if (string.IsNullOrEmpty(job.Settings.Home))
                job.Settings.Home = Path.FullName;

            job.VerifyExecutableExists();
            Jobs.Add(job);
        }
    }
}
