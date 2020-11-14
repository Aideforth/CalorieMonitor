using CalorieMonitor.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace CalorieMonitor.Core.Models
{
    public class CreateUserDTO
    {
        [Required]
        [MinLength(3)]
        [MaxLength(100)]
        public string FirstName { get; set; }

        [Required]
        [MinLength(3)]
        [MaxLength(100)]
        public string LastName { get; set; }

        [Required]
        [MinLength(3)]
        [MaxLength(100)]
        public string UserName { get; set; }

        [Required]
        [MinLength(8)]
        [MaxLength(25)]
        public string Password { get; set; }

        [Required]
        [MinLength(3)]
        [MaxLength(100)]
        [EmailAddress]
        public string EmailAddress { get; set; }

        [EnumDataType(typeof(UserRole), ErrorMessage = "Invalid User Role")]
        public UserRole Role { get; set; }

        [Required]
        [Range(1, 20000)]
        public double DailyCalorieLimit { get; set; }
    }
}
