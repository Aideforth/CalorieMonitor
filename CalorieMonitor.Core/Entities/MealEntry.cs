using CalorieMonitor.Core.Enums;
using System;
using System.Collections.Generic;

namespace CalorieMonitor.Core.Entities
{
    public class MealEntry : Entity
    {
        public User EntryUser { get; set; }
        public User EntryCreator { get; set; }
        public string Text { get; set; }
        public DateTime EntryDateTime { get; set; }
        public double Calories { get; set; }
        public CaloriesStatus CaloriesStatus { get; set; }
        public bool WithInDailyLimit { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        public long EntryUserId { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        public long EntryCreatorId { get; set; }
    }
}
