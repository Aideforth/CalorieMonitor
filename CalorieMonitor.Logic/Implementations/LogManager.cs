using CalorieMonitor.Logic.Interfaces;
using Serilog;
using System;
using System.Diagnostics;

namespace CalorieMonitor.Logic.Implementations
{
    public class LogManager : ILogManager
    {
        private readonly ILogger logger;
        public LogManager(ILogger logger)
        {
            this.logger = logger ?? throw new ArgumentNullException("logger");
        }

        public void LogException(Exception exception)
        {
            logger.Error($"{DateTime.Now} {exception.Message} | {exception.InnerException?.Message} at {exception.StackTrace}");
        }

        public void LogMessage(string message)
        {
            logger.Information(message);
        }
    }
}
