using System;

namespace CalorieMonitor.Core.Entities
{
    public class Entity
    {
        public long Id { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateUpdated { get; set; }
    }
}
