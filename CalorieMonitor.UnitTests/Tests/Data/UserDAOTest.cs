using CalorieMonitor.Core.Entities;
using CalorieMonitor.Core.Implementations;
using CalorieMonitor.Data.Implementations;
using CalorieMonitor.UnitTests.Mocks.Data;
using CalorieMonitor.UnitTests.Mocks.External;
using CalorieMonitor.UnitTests.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace CalorieMonitor.UnitTests.Tests.Data
{
    public class UserDAOTest
    {
        readonly UserDAO userDAO;
        readonly MockDbConnection mockDbConnection;
        readonly MockDbFilterQueryHandler mockDbFilterQueryHandler;
        readonly MockDbConnectionProvider mockDbConnectionprovider;
        readonly DateTime currentTime;
        readonly string SELECT_QUERY;
        readonly string COUNT_QUERY;
        readonly string PAGING_QUERY;
        private const string userName = "1-UserName";
        private const string email = "1_Email@mail.com";
        readonly SearchFilter filter = new SearchFilter();

        public UserDAOTest()
        {
            mockDbConnection = new MockDbConnection();
            mockDbFilterQueryHandler = new MockDbFilterQueryHandler();
            mockDbConnectionprovider = new MockDbConnectionProvider();
            mockDbConnectionprovider.MockGetDbConnectionAsync(mockDbConnection);

            userDAO = new UserDAO(mockDbConnectionprovider.Object, mockDbFilterQueryHandler.Object);

            SELECT_QUERY = (string)ReflectUtil.GetStaticValue(userDAO, "SELECT_QUERY");
            COUNT_QUERY = (string)ReflectUtil.GetStaticValue(userDAO, "COUNT_QUERY");
            PAGING_QUERY = (string)ReflectUtil.GetStaticValue(userDAO, "PAGING_QUERY");
            currentTime = DateTime.UtcNow;
        }

        [Fact]
        public void Constructor_NullIDbConnectionArgument_ThrowsException()
        {
            Exception exception = Record.Exception(() => new UserDAO(null, mockDbFilterQueryHandler.Object));
            Assert.IsType<ArgumentNullException>(exception);
            Assert.Equal("connectionProvider", (exception as ArgumentNullException).ParamName);
        }

        [Fact]
        public void Constructor_NullFilterQueryHandlerArgument_ThrowsException()
        {
            Exception exception = Record.Exception(() => new UserDAO(mockDbConnectionprovider.Object, null));
            Assert.IsType<ArgumentNullException>(exception);
            Assert.Equal("filterQueryHandler", (exception as ArgumentNullException).ParamName);
        }

        [Theory]
        [InlineData(4, 4)]
        [InlineData(10, 40)]
        [InlineData(0, 0)]
        public async Task SearchUsersAsync_Valid_ReturnsList(int recordCount, int totalRecords)
        {
            //Arrange
            string query = "where Clause @Name, @LocationId, @RecordDate";
            List<User> records = UserUtil.GenerateRecords(currentTime, recordCount);

            List<QueryParam> queryParams = new List<QueryParam>
            {
                new QueryParam { Name = "@Name", DbType = DbType.String, Value = "Name"},
                new QueryParam { Name = "@LocationId", DbType = DbType.Int64, Value = 2L},
                new QueryParam { Name = "@RecordDate", DbType = DbType.DateTime, Value = currentTime},
            };
            SetUpSearchUsersAsyncTest(totalRecords, query, records, queryParams);

            //Act
            SearchResult<User> response = await userDAO.SearchUsersAsync(filter, 4, 12);

            //Assert
            ValidateSearchUsersAsyncTest(recordCount, totalRecords, records, response, 8);
            Assert.Equal(2, mockDbConnection.Parameters.Count(c => c.ParameterName == "Name" && (string)c.Value == "Name" && c.DbType == DbType.String));
            Assert.Equal(2, mockDbConnection.Parameters.Count(c => c.ParameterName == "LocationId" && (long)c.Value == 2L && c.DbType == DbType.Int64));
            Assert.Equal(2, mockDbConnection.Parameters.Count(c => c.ParameterName == "RecordDate" && (DateTime)c.Value == currentTime && c.DbType == DbType.DateTime));
        }

        [Theory]
        [InlineData(4, 4)]
        [InlineData(10, 40)]
        [InlineData(0, 0)]
        public async Task SearchUsersAsync_ValidWithEmptySearchParameters_ReturnsList(int recordCount, int totalRecords)
        {
            //Arrange
            string query = "";
            List<User> records = UserUtil.GenerateRecords(currentTime, recordCount);
            SetUpSearchUsersAsyncTest(totalRecords, query, records, new List<QueryParam>());

            //Act
            SearchResult<User> emptyParamsresponse = await userDAO.SearchUsersAsync(filter, 4, 12);

            //Assert
            ValidateSearchUsersAsyncTest(recordCount, totalRecords, records, emptyParamsresponse, 2);
        }

        [Fact]
        public async Task SearchUsersAsync_Error_ThrowsException()
        {
            //arrange
            string query = "where Clause";

            mockDbFilterQueryHandler.MockGenerateQuery(filter, query, new List<QueryParam>());
            mockDbConnection.MockQueryAsyncWithException($"{SELECT_QUERY} {query} {PAGING_QUERY}", true);

            //Act
            Exception exception = await Record.ExceptionAsync(() => userDAO.SearchUsersAsync(filter, 2, 4));

            //Assert
            RunValidationForSqlException(exception);
        }

        [Fact]
        public async Task GetUserByEmailAsync_Valid_ReturnsUser()
        {
            //arrange
            User returnValue = UserUtil.GenerateRecords(currentTime, 1)[0];
            mockDbConnection.MockQueryFirstOrDefaultAsync($"{SELECT_QUERY} where EmailAddress = @EmailAddress", returnValue);

            //Act
            User response = await userDAO.GetUserByEmailAsync(email);

            //Assert
            Assert.NotNull(response);
            Action<User> verification = UserUtil.GenerateVerifications(new List<User> { returnValue })[0];
            verification(returnValue);
            RunValidationForGetUserByEmail(email);
        }

        [Fact]
        public async Task GetUserByEmailAsync_Valid_ReturnsNull()
        {
            //arrange
            mockDbConnection.MockQueryFirstOrDefaultAsync($"{SELECT_QUERY} where EmailAddress = @EmailAddress", null);

            //Act
            User response = await userDAO.GetUserByEmailAsync(email);

            //Assert
            Assert.Null(response);
            RunValidationForGetUserByEmail(email);
        }

        [Fact]
        public async Task GetUserByEmailAsync_Error_ThrowsException()
        {
            //arrange
            mockDbConnection.MockQueryFirstOrDefaultAsyncWithException($"{SELECT_QUERY} where EmailAddress = @EmailAddress");

            //Act
            Exception exception = await Record.ExceptionAsync(() => userDAO.GetUserByEmailAsync(email));

            //Assert
            RunValidationForSqlException(exception);
        }

        [Fact]
        public async Task GetUserByUserNameAsync_Valid_ReturnsUser()
        {
            //arrange
            User returnValue = UserUtil.GenerateRecords(currentTime, 1)[0];
            mockDbConnection.MockQueryFirstOrDefaultAsync($"{SELECT_QUERY} where UserName = @UserName", returnValue);

            //Act
            User response = await userDAO.GetUserByUserNameAsync(userName);

            //Assert
            Assert.NotNull(response);
            Action<User> verification = UserUtil.GenerateVerifications(new List<User> { returnValue })[0];
            verification(returnValue);
            RunValidationForGetUserByUserName(userName);
        }

        [Fact]
        public async Task GetUserByUserNameAsync_Valid_ReturnsNull()
        {
            //arrange
            mockDbConnection.MockQueryFirstOrDefaultAsync($"{SELECT_QUERY} where UserName = @UserName", null);

            //Act
            User response = await userDAO.GetUserByUserNameAsync(userName);

            //Assert
            Assert.Null(response);
            RunValidationForGetUserByUserName(userName);
        }

        [Fact]
        public async Task GetUserByUserNameAsync_Error_ThrowsException()
        {
            //arrange
            mockDbConnection.MockQueryFirstOrDefaultAsyncWithException($"{SELECT_QUERY} where UserName = @UserName");

            //Act
            Exception exception = await Record.ExceptionAsync(() => userDAO.GetUserByUserNameAsync(userName));

            //Assert
            RunValidationForSqlException(exception);
        }

        private void ValidateSearchUsersAsyncTest(int recordCount,
            int totalRecords,
            List<User> records,
            SearchResult<User> response,
            int parameterCount)
        {
            Assert.NotNull(response?.Results);
            Assert.Equal(recordCount, response.Results.Count);
            Assert.Equal(totalRecords, response.TotalCount);

            Action<User>[] verifications = UserUtil.GenerateVerifications(records);
            Assert.Collection(response.Results, verifications);
            if (recordCount == 0) Assert.Empty(response.Results);

            Assert.Equal(parameterCount, mockDbConnection.Parameters.Count);
            Assert.Equal(1, mockDbConnection.Parameters.Count(c => c.ParameterName == "Start" && (int)c.Value == 4 && c.DbType == DbType.Int32));
            Assert.Equal(1, mockDbConnection.Parameters.Count(c => c.ParameterName == "Limit" && (int)c.Value == 12 && c.DbType == DbType.Int32));

            mockDbFilterQueryHandler.RunVerification();
            RunVerification();
        }

        private void SetUpSearchUsersAsyncTest(int totalRecords, string query, List<User> records, List<QueryParam> queryParams)
        {
            mockDbFilterQueryHandler.MockGenerateQuery(filter, query, queryParams);
            mockDbConnection.MockQueryAsync($"{SELECT_QUERY} {query} {PAGING_QUERY}", records, true);
            mockDbConnection.MockExecuteScalarAsync($"{COUNT_QUERY} {query}", totalRecords, true);
        }

        private void RunVerification()
        {
            mockDbConnection.RunVerification();
            mockDbConnectionprovider.RunVerification();
        }

        private void RunValidationForSqlException(Exception exception)
        {
            Assert.IsType<SqlException>(exception);
            RunVerification();
        }

        private void RunValidationForGetUserByUserName(string userName)
        {
            Assert.Single(mockDbConnection.Parameters);
            Assert.Equal(1, mockDbConnection.Parameters.Count(c => c.ParameterName == "UserName" && (string)c.Value == userName));
            RunVerification();
        }

        private void RunValidationForGetUserByEmail(string email)
        {
            Assert.Single(mockDbConnection.Parameters);
            Assert.Equal(1, mockDbConnection.Parameters.Count(c => c.ParameterName == "EmailAddress" && (string)c.Value == email));
            RunVerification();
        }
    }
}
