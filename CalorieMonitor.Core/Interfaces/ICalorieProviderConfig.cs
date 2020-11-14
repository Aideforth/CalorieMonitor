namespace CalorieMonitor.Core.Interfaces
{
    public interface ICalorieProviderConfig
    {
        string ApiId { get; set; }
        string ApiKey { get; set; }
        string NutrientsUrl { get; set; }
    }
}
