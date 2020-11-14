using CalorieMonitor.Data.Interfaces;
using DbUp;
using Microsoft.Data.Sqlite;
using System;
using System.Data;
using System.Reflection;
using System.Threading.Tasks;

namespace CalorieMonitor.End2End.Implementations
{
    public class SqlLiteConnectionProvider : IDbConnectionProvider
    {
        readonly SqliteConnectionStringBuilder builder = new SqliteConnectionStringBuilder()
        {
            DataSource = Guid.NewGuid().ToString(),
            Mode = SqliteOpenMode.Memory,
            Cache = SqliteCacheMode.Shared
        };
        private SqliteConnection connection;
        public SqlLiteConnectionProvider()
        {
            SQLitePCL.raw.SetProvider(new SQLitePCL.SQLite3Provider_sqlite3());
        }
        public async Task<IDbConnection> GetDbConnectionAsync()
        {
            await SetUpConnection();
            return await Task.FromResult(connection);
        }

        private async Task SetUpConnection()
        {
            if (connection == null)
            {
                connection = new SqliteConnection(builder.ConnectionString);
                await connection.OpenAsync();
                SetUpDb(connection.ConnectionString);
            }
        }

        private bool SetUpDb(string connectionString)
        {
            var upgrader = DeployChanges.To.SQLiteDatabase(connectionString)
                .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
                .Build();
            var result = upgrader.PerformUpgrade();
            return result.Successful;
        }
    }
}
