using CalorieMonitor.Core.Enums;
using CalorieMonitor.Core.Interfaces;

namespace CalorieMonitor.Core.Implementations
{
    public class SearchFilter : IFilter
    {
        public IFilter LeftHandFilter { get; set; }
        public FilterOperation Operation { get; set; }
        public IFilter RightHandFilter { get; set; }
        public bool HasBrackets { get; set; }

        public string FilterString()
        {
            string query = $"{LeftHandFilter.FilterString()}:{Operation}:{RightHandFilter.FilterString()}";
            if (HasBrackets) query = $"({query})";
            return query;
        }
    }
}
