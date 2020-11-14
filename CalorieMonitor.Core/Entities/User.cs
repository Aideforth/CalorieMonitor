using CalorieMonitor.Core.Enums;

namespace CalorieMonitor.Core.Entities
{
    public class User : Entity
    {
        public UserRole Role { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string EmailAddress { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        public string Password { get; set; }
        public double DailyCalorieLimit { get; set; }
    }
}
