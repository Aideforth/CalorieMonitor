using System.Data;

namespace CalorieMonitor.Core.Implementations
{
    public struct QueryParam
    {
        public string Name { get; set; }
        public object Value { get; set; }
        public DbType DbType { get; set; }
    }
}
