using System;
using System.ComponentModel.DataAnnotations;

namespace CalorieMonitor.Core.Models
{
    public class PatchMealEntryDTO
    {
        [MinLength(3)]
        [MaxLength(200)]
        public string Text { get; set; }

        [Range(typeof(DateTime), "1/1/2018", "1/1/2100")]
        public DateTime? EntryDateTime { get; set; }

        [Range(0, 20000)]
        public double? Calories { get; set; }
    }
}
