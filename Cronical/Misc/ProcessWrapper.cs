using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Cronical.Logging;

namespace Cronical.Misc
{
  public class ProcessWrapper
  {
    private const int WmQuit = 0x0012;

    [return: MarshalAs(UnmanagedType.Bool)]
    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool PostThreadMessage(int threadId, int msg, IntPtr wParam, IntPtr lParam);

    protected readonly ProcessParameters Command;
    protected Process Process;
    protected readonly bool RedirectErrors;
    protected readonly bool RedirectAllOutput;
    protected readonly StringBuilder Result = new StringBuilder();

    public bool Running { get { return Process != null && !Process.HasExited; } }

    public ProcessWrapper(string command, string directory, bool redirectErrors, bool redirectAllOutput)
    {
      Command = new ProcessParameters(command, directory);
      RedirectAllOutput = redirectAllOutput;
      RedirectErrors = redirectErrors;
    }

    public string FetchResult()
    {
      var result = Result.ToString();
      Result.Clear();
      return result;
    }

    public void Start()
    {
      Process = new Process();

      try
      {
        Logger.Debug("Executing: " + Command.Executable);
        Logger.Debug("...parameters: " + Command.Parameters);
        Logger.Debug("...in directory: " + Command.Directory);

        Process.StartInfo.FileName = Command.Executable;
        Process.StartInfo.Arguments = Command.Parameters;
        Process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
        Process.StartInfo.CreateNoWindow = true;
        Process.StartInfo.WorkingDirectory = Command.Directory;
        Process.StartInfo.RedirectStandardInput = true;
        Process.StartInfo.RedirectStandardOutput = RedirectAllOutput;
        Process.StartInfo.RedirectStandardError = RedirectErrors;
        Process.StartInfo.UseShellExecute = false;

        Result.Clear();

        if (RedirectErrors)
        {
          Process.StartInfo.StandardErrorEncoding = Encoding.Default;
          Process.ErrorDataReceived += delegate(object sender, DataReceivedEventArgs args) {
            Result.AppendLine(args.Data);
          };
        }

        if (RedirectAllOutput)
        {
          Process.StartInfo.StandardOutputEncoding = Encoding.Default;
          Process.OutputDataReceived += delegate(object sender, DataReceivedEventArgs args) {
            Result.AppendLine(args.Data);
          };
        }

        if (!Process.Start())
          throw new Exception("Unable to spawn process.");

        if (RedirectErrors)
          Process.BeginErrorReadLine();

        if (RedirectAllOutput)
          Process.BeginOutputReadLine();
      }
      catch (Exception)
      {
        Process = null;
        throw;
      }
    }

    public void Stop()
    {
      Logger.Debug("Terminating: " + Command.Executable);

      // Maybe it's not even running?
      if (Process == null || Process.HasExited)
      {
        Logger.Debug("Stop: Service wasn't running, exiting right away");
        return;
      }

      // Try a few methods of signaling close. CloseMainWindow sends a WM_CLOSE message to the
      // main window, which usually closes an application; closing the StdIn pipe may also trigger
      // the end of a console program.

      Logger.Debug("Stop: Closing input stream and main window");
      Process.CloseMainWindow();
      Process.StandardInput.Close();
      if (Process.WaitForExit(3000))
      {
        Logger.Debug("Stop: Service terminated.");
        return;
      }

      // Be a little bit more blunt by mass posting WM_QUIT messages on the process threads. 
      // Also, start a subprocess that attaches to the child process' console and generates a CtrlC.
      // We can't do this in our own process because we need to detach from the console - and then
      // we can't use Console.WriteLine anymore.

      Logger.Debug("Stop: Sending quit message and Ctrl-Break");
      foreach (var id in Process.Threads.Cast<ProcessThread>().Select(thread => thread.Id).ToList())
        PostThreadMessage(id, WmQuit, (IntPtr)0, (IntPtr)0);
      InjectCtrlC.Break(Process.Id);
      if (Process.WaitForExit(3000))
      {
        Logger.Debug("Stop: Service terminated.");
        return;
      }

      // Still running? Force kill.

      Logger.Log("Service did not respond to end request, terminating forcibly.");
      Process.Kill();
      if (Process.WaitForExit(3000))
      {
        Logger.Debug("Stop: Service terminated.");
        return;
      }

      // If the client hasn't exited now, there's not much more to do.
      Logger.Warn("Service failed to terminate.");
    }

    public void WaitForEnd(int timeout)
    {
      if (Process == null)
        return;

      if (!Process.WaitForExit(timeout))
      {
        Logger.Warn("Job failed to end, terminating program");
        Logger.Catch(Stop);
      }

      // Give the system one second to finalize and write all async buffers
      Thread.Sleep(1000);
    }
  }
}
