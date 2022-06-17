using System.Runtime.InteropServices;
using System.Text;

namespace Cronical.Misc;

/// <summary>
/// Class that abstracts certain parameters for a process, such as the executable, command-line arguments,
/// locating the executable on disk etc.
/// </summary>
public class ProcessParameters
{
    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern int SearchPath(string lpPath, string lpFileName, string lpExtension, int nBufferLength,
        [MarshalAs(UnmanagedType.LPTStr)] StringBuilder lpBuffer, out IntPtr lpFilePart);

    public string Executable { get; }
    public string Directory { get; }
    public string Parameters { get; }

    public ProcessParameters(string command, string? home)
    {
        if (string.IsNullOrEmpty(home))
            home = System.IO.Directory.GetCurrentDirectory();

        // Extract the executable
        var cmd = StringParser.ExtractWord(ref command);

        // Get the path from Home=, or extract path from executable if not already assigned
        var cmdpath = Path.GetDirectoryName(cmd);
        if (!string.IsNullOrEmpty(cmdpath))
            home = Path.Combine(home, cmdpath);
        Directory = Path.GetFullPath(home);

        cmd = Path.GetFileName(cmd);
        if (string.IsNullOrEmpty(cmd) || string.IsNullOrEmpty(Directory))
            throw new Exception($"ProcessParameters initialization failed: Entries must be not null, Executable='{Executable}', Directory='{Directory}'");

        // We now have the Working Directory, search for the file
        Executable = FindExecutable(Directory, cmd);
        Parameters = command;
    }

    private string FindExecutable(string workingdir, string cmd)
    {
        // Search working directory for file, file.exe and file.cmd
        var filename = Path.Combine(workingdir, cmd);
        foreach (var f in new[] { filename, filename + ".exe", filename + ".cmd" })
            if (File.Exists(f))
                return f;

        // Search PATH
        foreach (var ext in new[] { ".exe", ".cmd" })
        {
            var sb = new StringBuilder(265);
            var n = SearchPath(null, cmd, ext, sb.Capacity, sb, out _);
            if (n != 0)
                return sb.ToString(0, n);
        }

        // No luck
        return filename;
    }
}