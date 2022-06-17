using System.Text;
using Cronical.Jobs;
using DotNetCommons.Security;
using Serilog.Core;

namespace Cronical.JobRunners;

public class WatchJobRunner : SingleJobRunner
{
    private ulong _lastCrc;

    public WatchJobRunner(Logger log) : base(log)
    {
    }

    public override bool ShouldRun(Job job)
    {
        if (IsRunning)
            return false;

        var crc = CalcDirectoryChecksum(job.Definition.Watch!);
        if (crc == null)
            return false;

        if (_lastCrc != crc.Value)
        {
            _lastCrc = crc.Value;
            return false;
        }

        return true;
    }

    private ulong? CalcDirectoryChecksum(DirectoryInfo directory)
    {
        var files = directory.EnumerateFiles("*", SearchOption.AllDirectories).ToList();
        if (!files.Any())
            return null;

        var sb = new StringBuilder();
        foreach (var file in files.OrderBy(x => x.Name))
        {
            try
            {
                using var fs = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.None);
                sb.AppendLine(string.Join("|", file.FullName, file.Length, file.LastWriteTime));
            }
            catch (IOException)
            {
                return null;
            }
        }

        var buffer = Encoding.UTF8.GetBytes(sb.ToString());
        return Crc64.ComputeChecksum(buffer);
    }
}