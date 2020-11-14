using CalorieMonitor.Core.Enums;
using CalorieMonitor.Core.Interfaces;

namespace CalorieMonitor.Core.Implementations
{
    public class FieldFilter : IFilter
    {
        public IFilter LeftHandFilter { get; set; }
        public IFilter RightHandFilter { get; set; }
        public bool HasBrackets { get; set; }
        public FilterOperation Operation { get; set; }
        public string FieldName { get; set; }
        public object FieldValue { get; set; }
        public FilterComparison Comparision { get; set; }

        public FieldFilter(string fieldName, FilterComparison comparision, object fieldValue)
        {
            Operation = FilterOperation.Field;
            FieldName = fieldName;
            FieldValue = fieldValue;
            Comparision = comparision;
            HasBrackets = true;
        }
        public string FilterString()
        {
            return $"({FieldName}:{Comparision}:{FieldValue}::{FieldValue.GetType().Name})";
        }
    }
}
