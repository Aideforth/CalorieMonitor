using System.ComponentModel.DataAnnotations;

namespace CalorieMonitor.Core.Models
{
    public class ChangePasswordRequest
    {
        [Required]
        public string OldPassword { get; set; }
        [Required]
        [MinLength(8)]
        [MaxLength(25)]
        public string NewPassword { get; set; }
    }
}
