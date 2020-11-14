using CalorieMonitor.Data.Interfaces;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace CalorieMonitor.Data.Implementations
{
    public class SqlConnectionProvider : IDbConnectionProvider
    {
        private readonly string connectionString;
        private SqlConnection connection;
        public SqlConnectionProvider(string connectionString)
        {
            this.connectionString = connectionString ?? throw new ArgumentNullException("connectionString");
        }
        public async Task<IDbConnection> GetDbConnectionAsync()
        {
            await SetUpConnection();
            return connection;
        }

        private async Task SetUpConnection()
        {
            if (connection == null)
            {
                connection = new SqlConnection(connectionString);
            }
            if (new ConnectionState[] { ConnectionState.Closed, ConnectionState.Broken }
                .Contains(connection.State))
            {
                await connection.OpenAsync();
            }
        }
    }
}
