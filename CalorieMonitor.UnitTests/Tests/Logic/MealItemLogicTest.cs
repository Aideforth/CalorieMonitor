using CalorieMonitor.Core.Entities;
using CalorieMonitor.Core.Enums;
using CalorieMonitor.Core.Exceptions;
using CalorieMonitor.Logic.Implementations;
using CalorieMonitor.UnitTests.Mocks.Data;
using CalorieMonitor.UnitTests.Mocks.Logic;
using CalorieMonitor.UnitTests.Utilities;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Xunit;

namespace CalorieMonitor.UnitTests.Tests.Logic
{
    public class MealItemLogicTest
    {
        readonly MockMealItemDAO mockMealItemDAO;
        readonly MockMealEntryDAO mockMealEntryDAO;
        readonly MockLogManager mockLogManager;
        readonly MockCalorieProviderService mockCalorieProviderService;
        readonly MealItemLogic itemLogic;
        readonly DateTime currentTime;
        private const long mealEntryId = 2;
        readonly MealEntry entry;

        public MealItemLogicTest()
        {
            mockCalorieProviderService = new MockCalorieProviderService();
            mockMealItemDAO = new MockMealItemDAO();
            mockMealEntryDAO = new MockMealEntryDAO();
            mockLogManager = new MockLogManager();

            itemLogic = new MealItemLogic(mockMealItemDAO.Object, mockMealEntryDAO.Object, mockCalorieProviderService.Object, mockLogManager.Object);
            currentTime = DateTime.UtcNow;
            entry = new MealEntry { Id = mealEntryId, EntryUser = new User { Id = 1 }, EntryCreator = new User { Id = 1 }, EntryDateTime = DateTime.Today };
        }

        [Fact]
        public void Constructor_NullIMealItemDAOArgument_ThrowsException()
        {
            Exception exception = Record.Exception(() => new MealItemLogic(null, mockMealEntryDAO.Object, mockCalorieProviderService.Object, mockLogManager.Object));
            Assert.IsType<ArgumentNullException>(exception);
            Assert.Equal("mealItemDAO", (exception as ArgumentNullException).ParamName);
        }

        [Fact]
        public void Constructor_NullIMealEntryDAOArgument_ThrowsException()
        {
            Exception exception = Record.Exception(() => new MealItemLogic(mockMealItemDAO.Object, null, mockCalorieProviderService.Object, mockLogManager.Object));
            Assert.IsType<ArgumentNullException>(exception);
            Assert.Equal("mealEntryDAO", (exception as ArgumentNullException).ParamName);
        }

        [Fact]
        public void Constructor_NullICalorieProviderServiceArgument_ThrowsException()
        {
            Exception exception = Record.Exception(() => new MealItemLogic(mockMealItemDAO.Object, mockMealEntryDAO.Object, null, mockLogManager.Object));
            Assert.IsType<ArgumentNullException>(exception);
            Assert.Equal("calorieProviderService", (exception as ArgumentNullException).ParamName);
        }

        [Fact]
        public void Constructor_NullILogManagerArgument_ThrowsException()
        {
            Exception exception = Record.Exception(() => new MealItemLogic(mockMealItemDAO.Object, mockMealEntryDAO.Object, mockCalorieProviderService.Object, null));
            Assert.IsType<ArgumentNullException>(exception);
            Assert.Equal("logManager", (exception as ArgumentNullException).ParamName);
        }

        [Fact]
        public async Task ProcessMealItemsAsync_Valid_ReturnsVoid()
        {
            //arrange
            List<MealItem> items = MealItemUtil.GenerateItems(currentTime, 3);
            mockMealItemDAO.MockInsertAsyncValidateWithName(items[0]);
            mockMealItemDAO.MockUpdateAsync(items[1]);
            mockMealItemDAO.MockDeleteAsync(3);
            mockMealItemDAO.MockGetItemsByMealEntryIdAsync(mealEntryId, new List<MealItem> { items[1], items[2] });
            mockMealEntryDAO.MockUpdateAsync(entry);
            mockCalorieProviderService.MockGetCalorieResultAsync(entry.Text, new CalorieServiceResult
            {
                Foods = new List<CalorieServiceFood>
                {
                    new CalorieServiceFood {FoodName = items[0].Name, Calories = 100},
                    new CalorieServiceFood {FoodName = items[1].Name, Calories = 200}
                }
            });
            mockMealEntryDAO.MockGetTotalCaloriesForUserInCurrentDateAsync(entry.EntryUser.Id, DateTime.Today, 100);
            entry.EntryUser.DailyCalorieLimit = 300;
            mockLogManager.MockLogMessage("For Meal Entry Id 2: 1 to update, 1 to delete, 1 to create");

            //Act
            await itemLogic.ProcessMealItemsAsync(entry);

            //Assert
            Assert.Equal(CaloriesStatus.AppProcessed, entry.CaloriesStatus);
            Assert.Equal(300, entry.Calories);
            Assert.False(entry.WithInDailyLimit);
            RunVerification();
        }

        [Fact]
        public async Task ProcessMealItemsAsync_ValidNoCalorieInfo_ReturnsVoid()
        {
            //arrange
            List<MealItem> items = MealItemUtil.GenerateItems(currentTime, 2);
            mockMealItemDAO.MockDeleteAsync(1);
            mockMealItemDAO.MockDeleteAsync(2);
            mockMealItemDAO.MockGetItemsByMealEntryIdAsync(mealEntryId, items);
            mockMealEntryDAO.MockUpdateAsync(entry);
            mockCalorieProviderService.MockGetCalorieResultAsync(entry.Text, new CalorieServiceResult
            {
                Foods = new List<CalorieServiceFood>()
            });
            mockMealEntryDAO.MockGetTotalCaloriesForUserInCurrentDateAsync(entry.EntryUser.Id, DateTime.Today, 100);
            entry.EntryUser.DailyCalorieLimit = 200;
            mockLogManager.MockLogMessage("For Meal Entry Id 2: 0 to update, 2 to delete, 0 to create");

            //Act
            await itemLogic.ProcessMealItemsAsync(entry);

            //Assert
            Assert.Equal(CaloriesStatus.NoInfoFound, entry.CaloriesStatus);
            Assert.Equal(0, entry.Calories);
            Assert.True(entry.WithInDailyLimit);
            RunVerification();
        }

        [Fact]
        public async Task ProcessMealItemsAsync_Error_ThrowsFailureException()
        {
            //arrange
            MealEntry entry = new MealEntry { Id = mealEntryId };
            mockCalorieProviderService.MockGetCalorieResultAsync(entry.Text, new CalorieServiceResult());
            mockMealItemDAO.MockGetItemsByMealEntryIdAsync(mealEntryId, null, true);
            mockLogManager.MockLogException<SqlException>();

            //Act
            Exception exception = await Record.ExceptionAsync(() => itemLogic.ProcessMealItemsAsync(entry));

            //Assert
            Assert.IsType<FailureException>(exception);
            Assert.Equal("An error occurred while processing meal items, please try again", exception.Message);

            RunVerification();
        }

        private void RunVerification()
        {
            mockMealItemDAO.RunVerification();
            mockMealEntryDAO.RunVerification();
            mockCalorieProviderService.RunVerification();
            mockLogManager.RunVerification();
        }
    }
}
