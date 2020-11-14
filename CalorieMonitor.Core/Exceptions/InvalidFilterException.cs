using System;

namespace CalorieMonitor.Core.Exceptions
{
    public class InvalidFilterException : Exception
    {
        public InvalidFilterException(string message) : base(message)
        {
        }
    }
}
