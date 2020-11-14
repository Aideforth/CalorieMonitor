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
    public class MealItemDAOTest
    {
        readonly MealItemDAO itemDAO;
        readonly MockDbConnection mockDbConnection;
        readonly MockDbConnectionProvider mockDbConnectionprovider;
        readonly DateTime currentTime;
        readonly string SELECT_QUERY = "Select * from MealItems";
        private const long mealEntryId = 2;

        public MealItemDAOTest()
        {
            mockDbConnection = new MockDbConnection();
            mockDbConnectionprovider = new MockDbConnectionProvider();
            mockDbConnectionprovider.MockGetDbConnectionAsync(mockDbConnection);

            itemDAO = new MealItemDAO(mockDbConnectionprovider.Object);

            SELECT_QUERY = (string)ReflectUtil.GetStaticValue(itemDAO, "SELECT_QUERY");
            currentTime = DateTime.UtcNow;
        }

        [Fact]
        public void Constructor_NullIDbConnectionArgument_ThrowsException()
        {
            Exception exception = Record.Exception(() => new MealItemDAO(null));
            Assert.IsType<ArgumentNullException>(exception);
            Assert.Equal("connectionProvider", (exception as ArgumentNullException).ParamName);
        }

        [Fact]
        public async Task GetItemsByMealEntryIdAsync_Valid_ReturnsItems()
        {
            //arrange
            ; List<MealItem> returnValues = MealItemUtil.GenerateItems(currentTime, 4);
            mockDbConnection.MockQueryAsync($"{SELECT_QUERY} where MealEntryId = @MealEntryId", returnValues);

            //Act
            List<MealItem> mealItems = await itemDAO.GetItemsByMealEntryIdAsync(mealEntryId);

            //Assert
            Assert.NotNull(mealItems);
            Action<MealItem>[] verifications = MealItemUtil.GenerateVerifications(returnValues);
            Assert.Collection(mealItems, verifications);
            RunVerification();
        }

        [Fact]
        public async Task GetItemsByMealEntryIdAsync_Valid_ReturnsEmptyList()
        {
            //arrange
            mockDbConnection.MockQueryAsync<MealItem>($"{SELECT_QUERY} where MealEntryId = @MealEntryId", new List<MealItem>());

            //Act
            List<MealItem> mealItems = await itemDAO.GetItemsByMealEntryIdAsync(mealEntryId);

            //Assert
            Assert.Empty(mealItems);
            RunVerification();
        }

        [Fact]
        public async Task GetUserByUserNameAsync_Error_ThrowsException()
        {
            //arrange
            mockDbConnection.MockQueryAsyncWithException($"{SELECT_QUERY} where MealEntryId = @MealEntryId");

            //Act
            Exception exception = await Record.ExceptionAsync(() => itemDAO.GetItemsByMealEntryIdAsync(mealEntryId));

            //Assert
            Assert.IsType<SqlException>(exception);
            RunVerification();
        }

        private void RunVerification()
        {
            mockDbConnection.RunVerification();
            mockDbConnectionprovider.RunVerification();
        }
    }
}
