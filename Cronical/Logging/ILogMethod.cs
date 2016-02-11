using System;

namespace Cronical.Logging
{
  public interface ILogMethod
  {
    bool Active { get; }
    void Open(LogConfiguration config);
    void Close();
    void Write(string text);
  }
}
