using System;

namespace CalorieMonitor.Core.Exceptions
{
    public class UnauthorizedException : Exception
    {
        public UnauthorizedException(string message = "Unauthorized") : base(message)
        {
        }
    }
}
