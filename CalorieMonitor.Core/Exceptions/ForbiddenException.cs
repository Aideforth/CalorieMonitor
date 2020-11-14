using System;

namespace CalorieMonitor.Core.Exceptions
{
    public class ForbiddenException : Exception
    {
        public ForbiddenException(string message = "Forbidden") : base(message)
        {
        }
    }
}
