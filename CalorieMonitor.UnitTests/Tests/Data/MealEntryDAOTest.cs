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
    public class MealEntryDAOTest
    {
        readonly MealEntryDAO entryDAO;
        readonly MockDbConnection mockDbConnection;
        readonly MockDbFilterQueryHandler mockDbFilterQueryHandler;
        readonly MockDbConnectionProvider mockDbConnectionprovider;
        readonly DateTime currentTime;
        readonly string SELECT_QUERY;
        readonly string COUNT_QUERY;
        readonly string PAGING_QUERY;
        readonly string TOTAL_CALORIES_IN_A_DAY_QUERY;

        public MealEntryDAOTest()
        {
            mockDbConnection = new MockDbConnection();
            mockDbFilterQueryHandler = new MockDbFilterQueryHandler();
            mockDbConnectionprovider = new MockDbConnectionProvider();
            mockDbConnectionprovider.MockGetDbConnectionAsync(mockDbConnection);

            entryDAO = new MealEntryDAO(mockDbConnectionprovider.Object, mockDbFilterQueryHandler.Object);

            SELECT_QUERY = (string)ReflectUtil.GetStaticValue(entryDAO, "SELECT_QUERY");
            COUNT_QUERY = (string)ReflectUtil.GetStaticValue(entryDAO, "COUNT_QUERY");
            PAGING_QUERY = (string)ReflectUtil.GetStaticValue(entryDAO, "PAGING_QUERY");
            TOTAL_CALORIES_IN_A_DAY_QUERY = (string)ReflectUtil.GetStaticValue(entryDAO, "TOTAL_CALORIES_IN_A_DAY_QUERY");
            currentTime = DateTime.UtcNow;
        }

        [Fact]
        public void Constructor_NullIDbConnectionArgument_ThrowsException()
        {
            Exception exception = Record.Exception(() => new MealEntryDAO(null, mockDbFilterQueryHandler.Object));
            Assert.IsType<ArgumentNullException>(exception);
            Assert.Equal("connectionProvider", (exception as ArgumentNullException).ParamName);
        }

        [Fact]
        public void Constructor_NullFilterQueryHandlerArgument_ThrowsException()
        {
            Exception exception = Record.Exception(() => new MealEntryDAO(mockDbConnectionprovider.Object, null));
            Assert.IsType<ArgumentNullException>(exception);
            Assert.Equal("filterQueryHandler", (exception as ArgumentNullException).ParamName);
        }

        [Theory]
        [InlineData(4, 4)]
        [InlineData(10, 40)]
        [InlineData(0, 0)]
        public async Task SearchEntriesAsync_Valid_ReturnsList(int recordCount, int totalEntries)
        {
            //Arrange
            SearchFilter filter = new SearchFilter();
            string query = "where Clause @Name, @EntryUserId, @EntryDateTime";

            List<QueryParam> queryParams = new List<QueryParam>
            {
                new QueryParam { Name = "@Name", DbType = DbType.String, Value = "Name"},
                new QueryParam { Name = "@EntryUserId", DbType = DbType.Int64, Value = 2L},
                new QueryParam { Name = "@EntryDateTime", DbType = DbType.DateTime, Value = currentTime},
            };
            List<MealEntry> entries = ArrangeSearchEntriesTest(recordCount, totalEntries, filter, query, queryParams);

            //Act
            SearchResult<MealEntry> response = await entryDAO.SearchEntriesAsync(filter, 2, 10);

            //Assert
            AssertResponseForSearchEntriesTest(response, entries, recordCount, totalEntries);
            Assert.Equal(2, mockDbConnection.Parameters.Count(c => c.ParameterName == "Name" && (string)c.Value == "Name" && c.DbType == DbType.String));
            Assert.Equal(2, mockDbConnection.Parameters.Count(c => c.ParameterName == "EntryUserId" && (long)c.Value == 2L && c.DbType == DbType.Int64));
            Assert.Equal(2, mockDbConnection.Parameters.Count(c => c.ParameterName == "EntryDateTime" && (DateTime)c.Value == currentTime && c.DbType == DbType.DateTime));
            RunCommonValidationsForSearchEntriesTest(2, 10, 8);
        }

        [Theory]
        [InlineData(4, 4)]
        [InlineData(10, 40)]
        [InlineData(0, 0)]
        public async Task SearchEntriesAsync_ValidWithEmptySearchParameters_ReturnsList(int recordCount, int totalEntries)
        {
            //Arrange
            SearchFilter filter = new SearchFilter();
            string query = "";
            List<MealEntry> entries = ArrangeSearchEntriesTest(recordCount, totalEntries, filter, query, new List<QueryParam>());

            //Act
            SearchResult<MealEntry> response = await entryDAO.SearchEntriesAsync(filter, 4, 12);

            //Assert
            AssertResponseForSearchEntriesTest(response, entries, recordCount, totalEntries);
            RunCommonValidationsForSearchEntriesTest(4, 12, 2);
        }

        [Fact]
        public async Task SearchEntriesAsync_Error_ThrowsException()
        {
            //arrange
            SearchFilter filter = new SearchFilter();
            string query = "where Clause";

            mockDbFilterQueryHandler.MockGenerateQuery(filter, query, new List<QueryParam>());
            mockDbConnection.MockQueryAsyncWithException($"{SELECT_QUERY} {query} {PAGING_QUERY}", true);

            //Act
            Exception exception = await Record.ExceptionAsync(() => entryDAO.SearchEntriesAsync(filter, 2, 4));

            //Assert
            Assert.IsType<SqlException>(exception);
            RunCommonValidationsForSearchEntriesTest(2, 4, 2);
        }

        [Fact]
        public async Task GetAsync_Valid_ReturnsItem()
        {
            //Arrange
            List<MealEntry> Entries = MealEntryUtil.GenerateEntries(currentTime, 1);
            mockDbConnection.MockQueryAsync($"{SELECT_QUERY} where Id = @Id", Entries, true);

            //Act
            MealEntry response = await entryDAO.GetAsync(1L);

            //Assert
            Assert.NotNull(response);
            Action<MealEntry> verifications = MealEntryUtil.GenerateVerifications(Entries)[0];
            verifications(Entries[0]);
            AssertResponseForGetAsyncTest(1L);
        }

        [Fact]
        public async Task GetAsync_Valid_ReturnsNull()
        {
            //Arrange
            List<MealEntry> Entries = MealEntryUtil.GenerateEntries(currentTime, 0);
            mockDbConnection.MockQueryAsync($"{SELECT_QUERY} where Id = @Id", Entries, true);

            //Act
            MealEntry response = await entryDAO.GetAsync(2L);

            //Assert
            Assert.Null(response);
            AssertResponseForGetAsyncTest(2L);
        }

        [Fact]
        public async Task GetAsync_Error_ThrowsException()
        {
            //arrange
            mockDbConnection.MockQueryAsyncWithException($"{SELECT_QUERY} where Id = @Id", true);

            //Act
            Exception exception = await Record.ExceptionAsync(() => entryDAO.GetAsync(1L));

            //Assert
            AssertResponseForGetAsyncTest(1L);
        }

        [Fact]
        public async Task GetTotalCaloriesForUserInCurrentDateAsync_Valid_ReturnsItems()
        {
            //arrange
            double sum = 200.02;
            MealEntry entry = MealEntryUtil.GenerateEntries(currentTime, 1)[0];
            mockDbConnection.MockExecuteScalarAsync(TOTAL_CALORIES_IN_A_DAY_QUERY, sum);

            //Act
            double totalCalories = await entryDAO.GetTotalCaloriesForUserInCurrentDateAsync(entry.Id, DateTime.Today);

            //Assert
            Assert.Equal(sum, totalCalories);
            RunCommonVerifications();
        }

        [Fact]
        public async Task GetTotalCaloriesForUserInCurrentDateAsync_Error_ThrowsException()
        {
            //arrange
            mockDbConnection.MockExecuteScalarAsyncWithException(TOTAL_CALORIES_IN_A_DAY_QUERY);

            //Act
            Exception exception = await Record.ExceptionAsync(() => entryDAO.GetTotalCaloriesForUserInCurrentDateAsync(0, DateTime.Today));

            //Assert
            Assert.IsType<SqlException>(exception);
            RunCommonVerifications();
        }

        private List<MealEntry> ArrangeSearchEntriesTest(int recordCount,
            int totalEntries,
            SearchFilter filter,
            string query,
            List<QueryParam> queryParams)
        {
            List<MealEntry> entries = MealEntryUtil.GenerateEntries(currentTime, recordCount);
            mockDbFilterQueryHandler.MockGenerateQuery(filter, query, queryParams);
            mockDbConnection.MockQueryAsync($"{SELECT_QUERY} {query} {PAGING_QUERY}", entries, true);
            mockDbConnection.MockExecuteScalarAsync($"{COUNT_QUERY} {query}", totalEntries, true);
            return entries;
        }

        private void AssertResponseForGetAsyncTest(long id)
        {
            Assert.Single(mockDbConnection.Parameters);
            Assert.Equal(1, mockDbConnection.Parameters.Count(c => c.ParameterName == "Id" && (long)c.Value == id && c.DbType == DbType.Int64));
            RunCommonVerifications();
        }

        private void AssertResponseForSearchEntriesTest(SearchResult<MealEntry> response,
            List<MealEntry> entries,
            int recordCount,
            int totalEntries)
        {
            Assert.NotNull(response?.Results);
            Assert.Equal(recordCount, response.Results.Count);
            Assert.Equal(totalEntries, response.TotalCount);

            Action<MealEntry>[] verifications = MealEntryUtil.GenerateVerifications(entries);
            Assert.Collection(response.Results, verifications);
            if (recordCount == 0) Assert.Empty(response.Results);
        }

        private void RunCommonValidationsForSearchEntriesTest(int startIndex, int limit, int parameterCount)
        {
            Assert.Equal(parameterCount, mockDbConnection.Parameters.Count);
            Assert.Equal(1, mockDbConnection.Parameters.Count(c =>
            {
                return c.ParameterName == "Start"
                && (int)c.Value == startIndex
                && c.DbType == DbType.Int32;
            }));
            Assert.Equal(1, mockDbConnection.Parameters.Count(c =>
            {
                return c.ParameterName == "Limit"
                && (int)c.Value == limit
                && c.DbType == DbType.Int32;
            }));
            mockDbFilterQueryHandler.RunVerification();
            RunCommonVerifications();
        }
        private void RunCommonVerifications()
        {
            mockDbConnection.RunVerification();
            mockDbConnectionprovider.RunVerification();
        }
    }
}
