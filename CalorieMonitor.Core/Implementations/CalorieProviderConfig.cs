using CalorieMonitor.Core.Interfaces;

namespace CalorieMonitor.Core.Implementations
{
    public class CalorieProviderConfig : ICalorieProviderConfig
    {
        public string ApiId { get; set; }
        public string ApiKey { get; set; }
        public string NutrientsUrl { get; set; }
    }
}
