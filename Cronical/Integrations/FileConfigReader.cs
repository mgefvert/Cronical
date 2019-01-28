using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Cronical.Configuration;
using Cronical.Jobs;
using Cronical.Misc;
using DotNetCommons.Logging;

namespace Cronical.Integrations
{
    public class FileConfigReader : IIntegration
    {
        private FileInfo _configFile;
        private DateTime _configTime;

        private class Command
        {
            public string Name;
            public string Value;
        }

        public FileConfigReader(string configFile)
        {
            _configFile = new FileInfo(configFile);
        }

        public bool Initialize(GlobalSettings settings, LogChannel logger)
        {
            return true;
        }

        public (JobLoadResult, List<Job>) FetchJobs(JobSettings defaultSettings)
        {
            if (!HasConfigChanged())
                return (JobLoadResult.NoChange, null);

            var config = LoadConfig();
            return (JobLoadResult.ReplaceJobs, config.Jobs);
        }

        public void Completed(Job job)
        {
        }

        public void Shutdown()
        {
        }

        private bool HasConfigChanged()
        {
            _configFile.Refresh();
            return _configFile.LastWriteTime > _configTime;
        }

        public Config LoadConfig()
        {
            _configTime = _configFile.LastWriteTime;
            using (var fs = new FileStream(_configFile.FullName, FileMode.Open, FileAccess.Read))
            {
                return LoadConfig(fs, _configFile.DirectoryName);
            }
        }

        internal static Config LoadConfig(Stream stream, string homedir)
        {
            var result = new Config();
            using (var reader = new StreamReader(stream))
            {
                var c = 0;
                while (!reader.EndOfStream)
                {
                    c++;
                    var line = PreprocessLine(reader.ReadLine());
                    if (string.IsNullOrEmpty(line))
                        continue;

                    var cmd = TryParseCommand(line);
                    if (cmd != null)
                    {
                        if (result.Settings.Set(cmd.Name, cmd.Value))
                            continue;

                        if (jobsettings.Set(cmd.Name, cmd.Value))
                            continue;
                    }

                    var job = TryParseJob(line, jobsettings);
                    if (job != null)
                    {
                        job.RecalcNextExecTime();
                        job.VerifyExecutableExists();
                        result.Jobs.Add(job);
                        continue;
                    }

                    Logger.Error($"Invalid configuration directive on line {c}: {line}");
                }
            }

            return result;
        }

        internal static string PreprocessLine(string line)
        {
            line = (line ?? "")
                .Replace("\t", " ")
                .Trim();

            if (string.IsNullOrEmpty(line))
                return null;

            var sb = new StringBuilder(line.Length);
            for (var i = 0; i < line.Length; i++)
            {
                var c = line[i];

                // \#
                if (c == '\\' && i < line.Length && line[i + 1] == '#')
                {
                    sb.Append('#');
                    i++;
                    continue;
                }

                if (c == '#')
                    break;

                sb.Append(c);
            }

            var result = sb.ToString().Trim();
            return string.IsNullOrEmpty(result) ? null : result;
        }

        private static Command TryParseCommand(string line)
        {
            var s = line.Trim();
            var cmd = StringParser.ExtractWord(ref s);
            var eq = StringParser.ExtractWord(ref s);

            return eq != "=" ? null : new Command { Name = cmd, Value = s };
        }

        public static Job ParseJob(string line, JobSettings settings = null)
        {
            return TryParseJob(line, settings ?? new JobSettings()) 
                   ?? throw new Exception($"Unable to interpret job definition: '{line}'");
        }

        private static Job TryParseJob(string line, JobSettings settings)
        {
            var definition = (line ?? "").Trim().Replace("\t", " ");
            if (string.IsNullOrWhiteSpace(definition) || definition[0] == '#')
                return null;

            if (definition[0] == '@')
            {
                var spec = StringParser.ExtractWord(ref definition).ToLower();

                switch (spec)
                {
                    case "@service":
                        return new ServiceJob
                        {
                            Command = definition,
                            Settings = settings.Clone()
                        };

                    case "@reboot":
                        return new CronJob
                        {
                            Command = definition,
                            Reboot = true,
                            Settings = settings.Clone()
                        };

                    case "@yearly": definition = "0 0 1 1 * " + definition; break;
                    case "@annually": definition = "0 0 1 1 * " + definition; break;
                    case "@monthly": definition = "0 0 1 * * " + definition; break;
                    case "@weekly": definition = "0 0 * * 0 " + definition; break;
                    case "@daily": definition = "0 0 * * * " + definition; break;
                    case "@hourly": definition = "0 * * * * " + definition; break;

                    default:
                        return null;
                }
            }

            try
            {
                var result = new CronJob
                {
                    Settings = settings.Clone(),
                    Minutes  = ParseValue(StringParser.ExtractWord(ref definition), 60, 0, 59, false),
                    Hours    = ParseValue(StringParser.ExtractWord(ref definition), 24, 0, 23, false),
                    Days     = ParseValue(StringParser.ExtractWord(ref definition), 31, 1, 31, false),
                    Months   = ParseValue(StringParser.ExtractWord(ref definition), 12, 1, 12, false),
                    Weekdays = ParseValue(StringParser.ExtractWord(ref definition),  7, 0,  6, true),
                    Command  = definition
                };

                return result;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static BitArray ParseValue(string spec, int len, int min, int max, bool weekdays)
        {
            try
            {
                var regex = new Regex(@"^(([\d\w]+)(-([\d\w]+))?|\*)(/(\d+))?$");
                var bits = new BitArray(len);

                foreach (var s in spec.Split(','))
                {
                    var match = regex.Match(s);
                    if (!match.Success)
                        throw new Exception();

                    var star  = match.Groups[1].Value == "*";
                    var start = match.Groups[2].Value;
                    var stop  = match.Groups[4].Value;
                    var every = match.Groups[6].Value;

                    if (weekdays)
                    {
                        start = TranslateWeekDay(start);
                        stop = TranslateWeekDay(stop);
                    }

                    if (every == "")
                        every = "1";

                    var istart = star ? min : int.Parse(start);
                    var istop = star ? max : int.Parse(stop == "" ? start : stop);
                    var ievery = int.Parse(every);

                    if (istart < min || istart > max || istop < min || istop > max || ievery < 1)
                        throw new Exception();

                    for (var i = istart - min; i <= istop - min; i += ievery)
                        bits.Set(i, true);
                }

                return bits;
            }
            catch (Exception)
            {
                throw new Exception("Can't make sense of '" + spec + "'");
            }
        }

        private static string TranslateWeekDay(string start)
        {
            int dummy;

            if (start == "7")
                return "0";

            if (int.TryParse(start, out dummy) || start == "*" || start == "")
                return start;

            switch (start.ToLower())
            {
                case "sun":
                case "sunday":
                    return "0";
                case "mon":
                case "monday":
                    return "1";
                case "tue":
                case "tuesday":
                    return "2";
                case "wed":
                case "wednesday":
                    return "3";
                case "thu":
                case "thursday":
                    return "4";
                case "fri":
                case "friday":
                    return "5";
                case "sat":
                case "saturday":
                    return "6";

                default:
                    throw new Exception("Unrecognized weekday name: " + start);
            }
        }
    }
}
