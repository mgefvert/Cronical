using System;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace Cronical.Logging
{
  public enum LogSeverity
  {
    Debug,
    Default,
    Notice,
    Warning,
    Error,
    None
  }

  public static class Logger
  {
    static Logger()
    {
      Method = new FileLogMethod();
      Configuration = new LogConfiguration();
    }

    public static LogConfiguration Configuration { get; private set; }
    public static ILogMethod Method { get; set; }

    [DebuggerStepThrough]
    public static bool Catch(Action code, LogSeverity severity = LogSeverity.Error)
    {
      try
      {
        code();
        return true;
      }
      catch (Exception e)
      {
        Write(severity, e.GetType().Name + ": " + e.Message);
        return false;
      }
    }

    public static void Close()
    {
      try
      {
        Method.Close();
      }
      catch
      {
      }
    }

    public static void Debug(string text, params object[] p)
    {
      Write(LogSeverity.Debug, text, p);
    }

    public static void Log(string text, params object[] p)
    {
      Write(LogSeverity.Default, text, p);
    }

    public static void Notice(string text, params object[] p)
    {
      Write(LogSeverity.Notice, text, p);
    }

    public static void Warn(string text, params object[] p)
    {
      Write(LogSeverity.Warning, text, p);
    }

    public static void Error(string text, params object[] p)
    {
      Write(LogSeverity.Error, text, p);
    }

    public static void Write(LogSeverity severity, string text, params object[] p)
    {
      InitConfig();
      if (severity < Configuration.Severity || severity == LogSeverity.None)
        return;

      text = EscapeText(string.Format(text, p));
      var time = DateTime.Now;
      var threadId = Thread.CurrentThread.ManagedThreadId;

      var severityText = GetSeverityText(severity);
      var threadText = threadId != Configuration.MainThreadId ? "[" + threadId + "] " : "";

      var output = threadText + text;
      if (severity == LogSeverity.Debug)
        output = " - " + output;

      // Echo to console should have a shorter timestamp
      if (Configuration.EchoToConsole)
      {
        var str = time.ToString("HH:mm:ss.fff") + " " + severityText.PadRight(8) + "  " + output;

        Console.ForegroundColor = GetSeverityColor(severity);
        (severity >= LogSeverity.Warning ? Console.Error : Console.Out).WriteLine(str);
        Console.ForegroundColor = ConsoleColor.Gray;
      }

      // Format for final output
      output = time.ToString("yyyyMMdd HHmmss.fff") + " " + severityText.PadRight(8) + "  " + output;

      try
      {
        if (!Method.Active)
          Method.Open(Configuration);

        Method.Write(output + "\r\n");
      }
      catch
      {
      }
    }

    private static string EscapeText(string text)
    {
      var result = new StringBuilder(text.Length);

      foreach (var c in text)
        if (c == '\r')
          result.Append("\r");
        else if (c == '\n')
          result.Append("\n");
        else if (c < 32)
          result.Append("#" + ((int)c).ToString("X2"));
        else
          result.Append(c);

      return result.ToString();
    }

    private static ConsoleColor GetSeverityColor(LogSeverity severity)
    {
      switch (severity)
      {
        case LogSeverity.Notice:
          return ConsoleColor.Green;
        case LogSeverity.Warning:
          return ConsoleColor.Yellow;
        case LogSeverity.Error:
          return ConsoleColor.Red;
        default:
          return ConsoleColor.Gray;
      }
    }

    private static string GetSeverityText(LogSeverity severity)
    {
      if (severity == LogSeverity.Default || severity == LogSeverity.Debug)
        return "";

      if (severity == LogSeverity.Notice)
        return "***";

      return severity.ToString().ToUpper();
    }

    private static void InitConfig()
    {
      if (!Configuration.Loaded)
        Configuration.Load();
    }
  }
}
