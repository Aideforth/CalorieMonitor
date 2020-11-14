using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CalorieMonitor.Core.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum UserRole
    {
        Regular,
        UserManager,
        Admin
    }
}
