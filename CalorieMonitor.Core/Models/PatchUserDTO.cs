using CalorieMonitor.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace CalorieMonitor.Core.Models
{
    public class PatchUserDTO
    {
        [MinLength(3)]
        [MaxLength(100)]
        public string FirstName { get; set; }

        [MinLength(3)]
        [MaxLength(100)]
        public string LastName { get; set; }

        [MinLength(3)]
        [MaxLength(100)]
        public string UserName { get; set; }

        [MinLength(3)]
        [MaxLength(100)]
        [EmailAddress]
        public string EmailAddress { get; set; }

        [EnumDataType(typeof(UserRole), ErrorMessage = "Invalid User Role")]
        public UserRole? Role { get; set; }

        [Range(1, 20000)]
        public double? DailyCalorieLimit { get; set; }
    }
}
