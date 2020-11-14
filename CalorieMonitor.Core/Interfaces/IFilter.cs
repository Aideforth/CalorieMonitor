using CalorieMonitor.Core.Enums;

namespace CalorieMonitor.Core.Interfaces
{
    public interface IFilter
    {
        IFilter LeftHandFilter { get; set; }
        FilterOperation Operation { get; set; }
        IFilter RightHandFilter { get; set; }
        bool HasBrackets { get; set; }
        string FilterString();
    }
}
