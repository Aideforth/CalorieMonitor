using System;

namespace CalorieMonitor.Logic.Interfaces
{
    public interface ILogManager
    {
        void LogException(Exception exception);
        void LogMessage(string message);
    }
}
