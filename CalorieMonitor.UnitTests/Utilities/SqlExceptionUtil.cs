using System.Data.SqlClient;
using System.Runtime.Serialization;

namespace CalorieMonitor.UnitTests.Utilities
{
    public class SqlExceptionUtil
    {
        public static SqlException Get()
        {
            return FormatterServices.GetUninitializedObject(typeof(SqlException))
                as SqlException;
        }
    }
}
