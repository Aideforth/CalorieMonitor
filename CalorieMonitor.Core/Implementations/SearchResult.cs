using CalorieMonitor.Core.Entities;
using System.Collections.Generic;

namespace CalorieMonitor.Core.Implementations
{
    public class SearchResult<T> where T : Entity
    {
        public List<T> Results { get; set; }
        public long TotalCount { get; set; }
    }
}
