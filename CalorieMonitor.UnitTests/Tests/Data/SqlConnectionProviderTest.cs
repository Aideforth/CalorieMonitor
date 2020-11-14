using CalorieMonitor.Data.Implementations;
using CalorieMonitor.UnitTests.Utilities;
using Microsoft.QualityTools.Testing.Fakes;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlClient.Fakes;
using System.Threading.Tasks;
using Xunit;

namespace CalorieMonitor.UnitTests.Tests.Data
{
    public class SqlConnectionProviderTest : IDisposable
    {
        readonly IDisposable shimsContext;

        public SqlConnectionProviderTest()
        {
            shimsContext = ShimsContext.Create();
        }

        [Fact]
        public void Constructor_NullArgument_ThrowsException()
        {
            Exception exception = Record.Exception(() => new SqlConnectionProvider(null));
            Assert.IsType<ArgumentNullException>(exception);
            Assert.Equal("connectionString", (exception as ArgumentNullException).ParamName);
        }

        [Fact]
        public async Task GetDbConnectionAsync_Valid_NoExisitingConnection()
        {
            //Arrange
            string connectionString = "Data Source=source";
            bool isOpenAsyncIsCalled = false;
            IDbConnection connection;

            ShimSqlConnection.AllInstances.OpenAsyncCancellationToken = (sqlConnection, token) =>
            {
                isOpenAsyncIsCalled = true;
                return Task.CompletedTask;
            };

            //Act
            connection = await new SqlConnectionProvider(connectionString).GetDbConnectionAsync();

            //Assert
            Assert.NotNull(connection);
            Assert.Equal(connectionString, connection.ConnectionString);
            Assert.True(isOpenAsyncIsCalled);
        }

        [Theory]
        [InlineData(ConnectionState.Open)]
        [InlineData(ConnectionState.Broken)]
        [InlineData(ConnectionState.Closed)]
        public async Task GetDbConnectionAsync_Valid_ExistingConnection(ConnectionState state)
        {
            //Arrange
            SqlConnection connectionProp = new SqlConnection();
            IDbConnection connection;
            int timesOpenAsyncIsCalled = 0;
            int timesExpected = state == ConnectionState.Open ? 0 : 1;
            SqlConnectionProvider provider = new SqlConnectionProvider("connectionString");
            ReflectUtil.SetPrivateValue(provider, "connection", connectionProp);

            ShimSqlConnection.AllInstances.StateGet = (sqlConnection) => state;
            ShimSqlConnection.AllInstances.OpenAsyncCancellationToken = (sqlConnection, token) =>
            {
                timesOpenAsyncIsCalled++;
                return Task.CompletedTask;
            };
            //Act
            connection = await provider.GetDbConnectionAsync();

            //Assert
            Assert.NotNull(connection);
            Assert.Same(connectionProp, connection);
            Assert.Equal(timesExpected, timesOpenAsyncIsCalled);
        }

        [Fact]
        public async Task GetDbConnectionAsync_Error_ThrowSqlException()
        {
            //Arrange
            string connectionString = "Data Source=source";
            SqlConnectionProvider provider = new SqlConnectionProvider(connectionString);
            Exception exception;

            ShimSqlConnection.AllInstances.OpenAsyncCancellationToken = (sqlConnection, token) =>
            {
                throw new InvalidOperationException();
            };

            //Act
            exception = await Record.ExceptionAsync(() => provider.GetDbConnectionAsync());

            //Assert
            Assert.IsType<InvalidOperationException>(exception);
        }

        public void Dispose()
        {
            shimsContext?.Dispose();
        }
    }
}
