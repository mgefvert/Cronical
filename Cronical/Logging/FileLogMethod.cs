using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Cronical.Logging
{
  public class FileLogMethod : ILogMethod
  {
    private LogConfiguration _configuration;
    private string _filename;
    private FileStream _stream;

    public bool Active { get; private set; }

    protected void CleanupExpiredFiles()
    {
      var allowedFiles = new List<string>();
      var allowedZipFiles = new List<string>();
      var directory = GetDirectory();

      // Build list of "allowed" file names
      var dt = DateTime.Now;
      var currentFile = GetLogFileName(dt).ToLower();
      for (var i = 1; i < _configuration.Retention; i++)
      {
        dt = dt.AddMonths(-1);
        allowedFiles.Add(GetLogFileName(dt).ToLower());
        allowedZipFiles.Add(GetLogFileName(dt).ToLower() + ".gz");
      }

      // Loop through the files in the folder and see what to do with them
      var files = Directory.GetFiles(directory, GetProcessName() + "-*").Select(f => new FileInfo(f));
      var regexLog = new Regex("^" + GetProcessName() + "-\\d{6}\\.log", RegexOptions.IgnoreCase);
      var regexZip = new Regex("^" + GetProcessName() + "-\\d{6}\\.log.gz", RegexOptions.IgnoreCase);
      foreach (var file in files.Where(f => regexLog.IsMatch(f.Name) || regexZip.IsMatch(f.Name)))
      {
        if (currentFile.Equals(file.Name, StringComparison.CurrentCultureIgnoreCase) || allowedZipFiles.Contains(file.Name.ToLower()))
          // Current file or allowed zip file, do nothing
          continue;

        if (allowedFiles.Contains(file.Name.ToLower()))
        {
          // Old file but not compressed, compress it
          CleanupCompressFile(file);
          continue;
        }

        // Not an allowed file either way - delete it
        CleanupDeleteFile(file);
      }
    }

    private void CleanupCompressFile(FileInfo file)
    {
      using (var fs = new FileStream(file.Name, FileMode.Open))
      using (var gzfile = new FileStream(file.Name + ".gz", FileMode.Create))
      using (var gz = new GZipStream(gzfile, CompressionMode.Compress))
      {
        fs.CopyTo(gz);
      }

      CleanupDeleteFile(file);
    }

    protected void CleanupDeleteFile(FileInfo file)
    {
      try
      {
        Logger.Log("Deleting old log file '" + file.FullName + "'");
        file.Delete();
      }
      catch (IOException e)
      {
        Logger.Log("Unable to delete old log file: " + e.Message);
      }
    }

    protected string GetDirectory()
    {
      return string.IsNullOrEmpty(_configuration.Path)
        ? Directory.GetCurrentDirectory()
        : _configuration.Path;
    }

    private string GetLogFileName(DateTime time)
    {
      return GetProcessName() + "-" + time.ToString("yyyyMM") + ".log";
    }

    protected string GetProcessName()
    {
      return string.IsNullOrEmpty(_configuration.ProcessName) 
        ? Assembly.GetEntryAssembly().GetName().Name 
        : _configuration.ProcessName.Trim();
    }

    public void Open(LogConfiguration configuration)
    {
      _configuration = configuration;
      _stream = null;
      _filename = GetLogFileName(DateTime.Now);

      var directory = GetDirectory();
      if (directory != null && !Directory.Exists(directory))
        Directory.CreateDirectory(directory);

      _stream = new FileStream(Path.Combine(directory, _filename), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
      Active = true;

      // Must come after opening since CheckExpiredFiles actually logs text
      CleanupExpiredFiles();
    }

    public void Close()
    {
      Active = false;
      _filename = null;

      if (_stream == null)
        return;

      _stream.Close();
      _stream = null;
    }

    public void Write(string text)
    {
      // Check if the file needs to be reopened
      if (_filename != GetLogFileName(DateTime.Now) || _stream == null || _stream.CanWrite == false)
      {
        Close();
        Open(_configuration);
      }

      if (_stream == null || _stream.CanWrite == false)
        return;

      var buffer = Encoding.Default.GetBytes(text);

      lock (_stream)
      {
        _stream.Seek(0, SeekOrigin.End);
        var pos = _stream.Position;
        var len = text.Length;

        _stream.Lock(pos, len + 100);
        try
        {
          _stream.Seek(0, SeekOrigin.End);
          _stream.Write(buffer, 0, buffer.Length);
          _stream.Flush();
        }
        finally
        {
          _stream.Unlock(pos, len + 100);
        }
      }
    }
  }
}
