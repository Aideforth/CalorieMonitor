using CalorieMonitor.Core.Entities;
using CalorieMonitor.Core.Enums;
using CalorieMonitor.Core.Exceptions;
using CalorieMonitor.Core.Implementations;
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
    public class MealEntryLogicTest
    {
        readonly MockMealEntryDAO mockMealEntryDAO;
        readonly MockLogManager mockLogManager;
        readonly MockMealItemLogic mockMealItemLogic;
        readonly MockBackgroundJobClient mockBackgroundJobClient;
        readonly MealEntryLogic entryLogic;
        readonly DateTime currentTime;
        readonly MealEntry entry;
        readonly MealEntry entryCheck;

        public MealEntryLogicTest()
        {
            mockMealItemLogic = new MockMealItemLogic();
            mockMealEntryDAO = new MockMealEntryDAO();
            mockLogManager = new MockLogManager();
            mockBackgroundJobClient = new MockBackgroundJobClient();

            entryLogic = new MealEntryLogic(mockMealEntryDAO.Object, mockMealItemLogic.Object, mockLogManager.Object, mockBackgroundJobClient.Object);
            currentTime = DateTime.UtcNow;
            entry = MealEntryUtil.GenerateEntries(currentTime, 1)[0];
            entryCheck = MealEntryUtil.GenerateEntries(currentTime, 1)[0];
        }

        [Fact]
        public void Constructor_NullIMealEntryDAOArgument_ThrowsException()
        {
            Exception exception = Record.Exception(() => new MealEntryLogic(null, mockMealItemLogic.Object, mockLogManager.Object, mockBackgroundJobClient.Object));
            Assert.IsType<ArgumentNullException>(exception);
            Assert.Equal("mealEntryDAO", (exception as ArgumentNullException).ParamName);
        }

        [Fact]
        public void Constructor_NullICalorieServiceProviderArgument_ThrowsException()
        {
            Exception exception = Record.Exception(() => new MealEntryLogic(mockMealEntryDAO.Object, null, mockLogManager.Object, mockBackgroundJobClient.Object));
            Assert.IsType<ArgumentNullException>(exception);
            Assert.Equal("mealItemLogic", (exception as ArgumentNullException).ParamName);
        }

        [Fact]
        public void Constructor_NullILogManagerArgument_ThrowsException()
        {
            Exception exception = Record.Exception(() => new MealEntryLogic(mockMealEntryDAO.Object, mockMealItemLogic.Object, null, mockBackgroundJobClient.Object));
            Assert.IsType<ArgumentNullException>(exception);
            Assert.Equal("logManager", (exception as ArgumentNullException).ParamName);
        }

        [Fact]
        public void Constructor_NullIBackgroundJobClientArgument_ThrowsException()
        {
            Exception exception = Record.Exception(() => new MealEntryLogic(mockMealEntryDAO.Object, mockMealItemLogic.Object, mockLogManager.Object, null));
            Assert.IsType<ArgumentNullException>(exception);
            Assert.Equal("_jobClient", (exception as ArgumentNullException).ParamName);
        }

        [Fact]
        public async Task SaveAsync_Valid_ReturnsEntry()
        {
            //arrange
            mockMealEntryDAO.MockInsertAsync(entry);
            mockBackgroundJobClient.MockEnqueue();

            //Act
            MealEntry response = await entryLogic.SaveAsync(entry);

            //Assert
            Assert.NotNull(response);
            Assert.Same(entry, response);
            Assert.True(currentTime < response.DateCreated);

            //validate entrys with new date
            entryCheck.DateCreated = response.DateCreated;

            var MealEntryVer = MealEntryUtil.GenerateVerifications(new List<MealEntry> { entryCheck })[0];
            MealEntryVer(response);
            RunVerification();
        }

        [Fact]
        public async Task SaveAsync_ErrorDateTimeInFuture_ThrowsBusinessException()
        {
            //arrange
            mockLogManager.MockLogException<BusinessException>();
            entry.EntryDateTime = DateTime.UtcNow.AddSeconds(5);

            //Act
            Exception exception = await Record.ExceptionAsync(() => entryLogic.SaveAsync(entry));

            //Assert
            Assert.IsType<BusinessException>(exception);
            Assert.Equal("Entry DateTime is in the future", exception.Message);

            RunVerification();
        }

        [Fact]
        public async Task SaveAsync_Error_ThrowsFailureException()
        {
            //arrange
            mockLogManager.MockLogException<SqlException>();
            mockMealEntryDAO.MockInsertAsync(entry, true);

            //Act
            Exception exception = await Record.ExceptionAsync(() => entryLogic.SaveAsync(entry));

            //Assert
            Assert.IsType<FailureException>(exception);
            Assert.Equal("An error occurred while saving the entry, please try again", exception.Message);

            RunVerification();
        }

        [Theory]
        [InlineData(100)]
        [InlineData(1000)]
        public async Task SaveAsync_ValidWithoutUsingBackgroundClient_ReturnsEntries(double dailyLimit)
        {
            //arrange
            mockMealEntryDAO.MockInsertAsync(entry);
            mockMealEntryDAO.MockGetTotalCaloriesForUserInCurrentDateAsync(entry.EntryUser.Id, DateTime.Today, 100);
            entry.CaloriesStatus = CaloriesStatus.CustomerProvided;
            entry.Calories = 50;
            entry.EntryDateTime = DateTime.Today;
            entry.EntryUser.DailyCalorieLimit = dailyLimit;

            //Act
            MealEntry response = await entryLogic.SaveAsync(entry);

            //Assert
            Assert.NotNull(response);
            Assert.Same(entry, response);
            Assert.True(currentTime < response.DateCreated);

            if (dailyLimit > 150)
                Assert.True(entry.WithInDailyLimit);
            else
                Assert.False(entry.WithInDailyLimit);

            //validate entrys with new date

            RunVerification();
        }


        [Fact]
        public async Task UpdateAsync_Valid_ReturnsEntry()
        {
            //arrange
            mockMealEntryDAO.MockUpdateAsync(entry);
            mockBackgroundJobClient.MockEnqueue();

            //Act
            MealEntry response = await entryLogic.UpdateAsync(entry);

            //Assert
            Assert.NotNull(response);
            Assert.Same(entry, response);
            Assert.True(currentTime < response.DateUpdated);

            //validate entrys with new date
            entryCheck.DateUpdated = response.DateUpdated;

            var MealEntryVer = MealEntryUtil.GenerateVerifications(new List<MealEntry> { entryCheck })[0];
            MealEntryVer(response);
            RunVerification();
        }

        [Fact]
        public async Task UpdateAsync_Error_ThrowsFailureException()
        {
            //arrange
            mockMealEntryDAO.MockUpdateAsync(entry, true);
            mockLogManager.MockLogException<SqlException>();

            //Act
            Exception exception = await Record.ExceptionAsync(() => entryLogic.UpdateAsync(entry));

            //Assert
            Assert.IsType<FailureException>(exception);
            Assert.Equal("An error occurred while updating the entry, please try again", exception.Message);

            RunVerification();
        }

        [Fact]
        public async Task UpdateAsync_ErrorDateTimeInFuture_ThrowsBusinessException()
        {
            //arrange
            mockLogManager.MockLogException<BusinessException>();
            entry.EntryDateTime = DateTime.UtcNow.AddSeconds(5);

            //Act
            Exception exception = await Record.ExceptionAsync(() => entryLogic.UpdateAsync(entry));

            //Assert
            Assert.IsType<BusinessException>(exception);
            Assert.Equal("Entry DateTime is in the future", exception.Message);

            RunVerification();
        }

        [Fact]
        public async Task GetAsync_Valid_ReturnsEntry()
        {
            //arrange
            long id = 2;

            mockMealEntryDAO.MockGetEntryAsync(id, entry);

            //Act
            MealEntry response = await entryLogic.GetAsync(id);

            //Assert
            Assert.NotNull(response);
            var MealEntryVer = MealEntryUtil.GenerateVerifications(new List<MealEntry> { entryCheck })[0];
            MealEntryVer(response);

            RunVerification();
        }
        [Fact]
        public async Task GetAsync_Valid_ReturnsNull()
        {
            //arrange
            long id = 2;
            mockMealEntryDAO.MockGetEntryAsync(id, null);

            //Act
            MealEntry response = await entryLogic.GetAsync(id);

            //Assert
            Assert.Null(response);
            RunVerification();
        }

        [Fact]
        public async Task GetAsync_Error_ThrowsFailureException()
        {
            //arrange
            long id = 2;
            mockMealEntryDAO.MockGetEntryAsync(id, null, true);
            mockLogManager.MockLogException<SqlException>();

            //Act
            Exception exception = await Record.ExceptionAsync(() => entryLogic.GetAsync(id));

            //Assert
            Assert.IsType<FailureException>(exception);
            Assert.Equal("An error occurred while retrieving the entry, please try again", exception.Message);

            RunVerification();
        }

        [Fact]
        public async Task DeleteAsync_Valid_ReturnsTrue()
        {
            //arrange
            long id = 2;

            mockMealEntryDAO.MockDeleteAsync(id);

            //Act
            bool response = await entryLogic.DeleteAsync(id);

            //Assert
            Assert.True(response);

            RunVerification();
        }

        [Fact]
        public async Task DeleteAsync_Error_ThrowsFailureException()
        {
            //arrange
            long id = 2;
            mockMealEntryDAO.MockDeleteAsync(id, true);
            mockLogManager.MockLogException<SqlException>();

            //Act
            Exception exception = await Record.ExceptionAsync(() => entryLogic.DeleteAsync(id));

            //Assert
            Assert.IsType<FailureException>(exception);
            Assert.Equal("An error occurred while deleting the entry, please try again", exception.Message);

            RunVerification();
        }

        [Fact]
        public async Task SearchEntriesAsync_Valid_ReturnsEntry()
        {
            //arrange
            SearchFilter filter = new SearchFilter();
            List<MealEntry> entrys = MealEntryUtil.GenerateEntries(currentTime, 4);
            List<MealEntry> entrysCheck = MealEntryUtil.GenerateEntries(currentTime, 4);
            SearchResult<MealEntry> returnThis = new SearchResult<MealEntry> { Results = entrys, TotalCount = 20 };

            mockMealEntryDAO.MockSearchEntriesAsync(filter, 1, 2, returnThis);

            //Act
            SearchResult<MealEntry> response = await entryLogic.SearchEntriesAsync(filter, 1, 2);

            //Assert
            Assert.NotNull(response);
            Assert.NotNull(response.Results);
            Assert.Equal(20, response.TotalCount);

            var MealEntryVer = MealEntryUtil.GenerateVerifications(entrysCheck);
            Assert.Collection(response.Results, MealEntryVer);

            RunVerification();
        }

        [Fact]
        public async Task SearchEntriesAsync_Error_ThrowsFailureException()
        {
            //arrange
            SearchFilter filter = new SearchFilter();
            mockMealEntryDAO.MockSearchEntriesAsync(filter, 1, 2, null, true);
            mockLogManager.MockLogException<SqlException>();

            //Act
            Exception exception = await Record.ExceptionAsync(() => entryLogic.SearchEntriesAsync(filter, 1, 2));

            //Assert
            Assert.IsType<FailureException>(exception);
            Assert.Equal("An error occurred while retrieving the entries, please try again", exception.Message);

            RunVerification();
        }

        private void RunVerification()
        {
            mockMealEntryDAO.RunVerification();
            mockMealItemLogic.RunVerification();
            mockLogManager.RunVerification();
            mockBackgroundJobClient.RunVerification();

        }
    }
}
