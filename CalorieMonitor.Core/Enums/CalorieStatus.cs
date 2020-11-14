using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CalorieMonitor.Core.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum CaloriesStatus
    {
        Pending,
        CustomerProvided,
        NoInfoFound,
        AppProcessed
    }
}
