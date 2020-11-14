using System;
namespace CalorieMonitor.Core.Exceptions
{
    public class FailureException : Exception
    {
        public FailureException(string message) : base(message)
        {
        }
    }
}
