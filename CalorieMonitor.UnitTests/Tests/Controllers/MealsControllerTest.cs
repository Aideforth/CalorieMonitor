using CalorieMonitor.Controllers;
using CalorieMonitor.Core.Entities;
using CalorieMonitor.Core.Enums;
using CalorieMonitor.Core.Exceptions;
using CalorieMonitor.Core.Implementations;
using CalorieMonitor.Core.Models;
using CalorieMonitor.UnitTests.Mocks.Logic;
using CalorieMonitor.UnitTests.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace CalorieMonitor.UnitTests.Tests.Controllers
{
    public class MealsControllerTest : ControllerTestBase
    {
        readonly MockMealEntryLogic mockMealEntryLogic;
        readonly MockUserLogic mockUserLogic;
        readonly MockSearchFilterHandler mockSearchFilterHandler;
        readonly MealsController mealsController;
        readonly DateTime currentTime;
        readonly MealEntry mealEntry;
        private const string filterString = "(EntryDate eq '2019-02-12')";
        SearchFilter filter;
        List<MealEntry> entrysCheck;
        SearchResult<MealEntry> returnThis;

        public MealsControllerTest()
        {
            mockMealEntryLogic = new MockMealEntryLogic();
            mockUserLogic = new MockUserLogic();
            mockSearchFilterHandler = new MockSearchFilterHandler();

            mealsController = new MealsController(mockMealEntryLogic.Object, mockUserLogic.Object, mockSearchFilterHandler.Object);
            currentTime = DateTime.UtcNow;
            mealEntry = MealEntryUtil.GenerateEntries(currentTime, 1)[0];
        }

        [Fact]
        public void Constructor_NullIMealEntryLogicArgument_ThrowsException()
        {
            Exception exception = Record.Exception(() => new MealsController(null, mockUserLogic.Object, mockSearchFilterHandler.Object));
            Assert.IsType<ArgumentNullException>(exception);
            Assert.Equal("mealEntryLogic", (exception as ArgumentNullException).ParamName);
        }

        [Fact]
        public void Constructor_NullIUserLogicArgument_ThrowsException()
        {
            Exception exception = Record.Exception(() => new MealsController(mockMealEntryLogic.Object, null, mockSearchFilterHandler.Object));
            Assert.IsType<ArgumentNullException>(exception);
            Assert.Equal("userLogic", (exception as ArgumentNullException).ParamName);
        }

        [Fact]
        public void Constructor_NullISearchFilterHandlerArgument_ThrowsException()
        {
            Exception exception = Record.Exception(() => new MealsController(mockMealEntryLogic.Object, mockUserLogic.Object, null));
            Assert.IsType<ArgumentNullException>(exception);
            Assert.Equal("searchFilterHandler", (exception as ArgumentNullException).ParamName);
        }

        [Fact]
        public async Task AddMealEntry_NullBody_Returns400()
        {
            //Act
            IActionResult response = await mealsController.AddMealEntry(null);

            //Assert
            ValidateBadRequest(response, "Result", "No Payload was sent");

            RunVerifications();
        }

        [Fact]
        public async Task AddMealEntry_InvalidModel_Returns400()
        {
            //Arrange
            mealsController.ModelState.AddModelError("Test", "Test Error");

            //Act
            IActionResult response = await mealsController.AddMealEntry(new CreateMealEntryDTO());

            //Assert
            ValidateBadRequest(response, "Test", "Test Error");

            RunVerifications();
        }

        [Fact]
        public async Task AddMealEntry_AdminCreateInvalidUser_Returns400()
        {
            //Arrange
            SetUpController("2", UserRole.Admin);
            mockUserLogic.MockGetAsync(2, null);
            CreateMealEntryDTO entry = new CreateMealEntryDTO { EntryUser = new EntryUserDTO { Id = 2 } };

            //Act
            IActionResult response = await mealsController.AddMealEntry(entry);

            //Assert
            ValidateBadRequest(response, "EntryUserId", "The EntryUserId provided does not exist");

            RunVerifications();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(200.4)]
        public async Task AddMealEntry_Valid_Returns201(double calories)
        {
            //Arrange
            SetUpController(mealEntry.EntryUserId.ToString(), UserRole.Regular);
            mockUserLogic.MockGetAsync(mealEntry.EntryUserId, new User(), null, 2);
            mockMealEntryLogic.MockSaveAsync();
            mealEntry.Calories = calories;
            CreateMealEntryDTO entry = ConvertToDTO(mealEntry);
            entry.EntryUser = null;

            //Act
            IActionResult response = await mealsController.AddMealEntry(entry);

            //Assert
            ValidateAddMealEntryTest(entry, response, calories);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(100.4)]
        public async Task AddMealEntry_ValidAdmin_Returns201(double calories)
        {
            //Arrange
            SetUpController(mealEntry.EntryUserId.ToString(), UserRole.Admin);
            mockUserLogic.MockGetAsync(mealEntry.EntryUserId, new User(), null, 2);
            mockMealEntryLogic.MockSaveAsync();
            CreateMealEntryDTO entry = ConvertToDTO(mealEntry);
            entry.Calories = calories;

            //Act
            IActionResult response = await mealsController.AddMealEntry(entry);

            //Assert
            ValidateAddMealEntryTest(entry, response, calories);
        }

        [Fact]
        public async Task AddMealEntry_HandleBusinessException_Returns400()
        {
            //Arrange
            SetUpController("2", UserRole.Regular);
            mockUserLogic.MockGetAsync(2, null, new BusinessException("New Error"));

            //Act
            IActionResult response = await mealsController.AddMealEntry(new CreateMealEntryDTO());

            //Assert
            ValidateApiError<ObjectResult>(response, 400, "New Error");

            RunVerifications();
        }

        [Fact]
        public async Task AddMealEntry_HandleFailureException_Returns500()
        {
            //Arrange
            SetUpController("2", UserRole.Regular);
            mockUserLogic.MockGetAsync(2, null, new FailureException("New Error"));

            //Act
            IActionResult response = await mealsController.AddMealEntry(new CreateMealEntryDTO());

            //Assert
            ValidateApiError<ObjectResult>(response, 500, "New Error");
            RunVerifications();
        }

        [Fact]
        public async Task AddMealEntry_HandleOtherException_Returns500()
        {
            //Arrange
            SetUpController("2", UserRole.Regular);
            mockUserLogic.MockGetAsync(2, null, new Exception("New Error"));

            //Act
            IActionResult response = await mealsController.AddMealEntry(new CreateMealEntryDTO());

            //Assert
            ValidateApiError<ObjectResult>(response, 500, "An error occurred, please try again");

            RunVerifications();
        }

        [Fact]
        public async Task PatchMealEntry_InvalidId_Returns404()
        {
            //Assert
            long id = 2;
            mockMealEntryLogic.MockGetAsync(id, null);
            //Act
            IActionResult response = await mealsController.PatchMealEntry(null, id);

            //Assert
            ValidateApiError<NotFoundObjectResult>(response, 404, "Meal Entry Not Found");

            RunVerifications();
        }

        [Fact]
        public async Task PatchMealEntry_Forbidden_Returns403()
        {
            //Arrange
            long id = 2;
            mockMealEntryLogic.MockGetAsync(id, new MealEntry { EntryUserId = 4 });
            SetUpController("3", UserRole.Regular);

            //Act
            IActionResult response = await mealsController.PatchMealEntry(null, id);

            //Assert
            ValidateApiError<ObjectResult>(response, 403, "Forbidden");

            RunVerifications();
        }

        [Fact]
        public async Task PatchMealEntry_NullBody_Returns400()
        {
            //Arrange
            long id = 2;
            mockMealEntryLogic.MockGetAsync(id, new MealEntry { EntryUserId = 4 });
            SetUpController("4", UserRole.Regular);

            //Act
            IActionResult response = await mealsController.PatchMealEntry(null, 2);

            //Assert
            ValidateBadRequest(response, "Result", "No Payload was sent");

            RunVerifications();
        }

        [Fact]
        public async Task PatchMealEntry_InvalidModel_Returns400()
        {
            //Arrange
            long id = 2;
            mockMealEntryLogic.MockGetAsync(id, new MealEntry { EntryUserId = 4 });
            SetUpController("4", UserRole.Regular);
            mealsController.ModelState.AddModelError("Test", "Test Error");

            //Act
            IActionResult response = await mealsController.PatchMealEntry(new PatchMealEntryDTO(), 2);

            //Assert
            ValidateBadRequest(response, "Test", "Test Error");

            RunVerifications();
        }


        [Theory]
        [InlineData(0)]
        [InlineData(200.4)]
        public async Task PatchMealEntry_Valid_Returns200(double calories)
        {
            //Arrange
            long id = mealEntry.Id;
            mockMealEntryLogic.MockGetAsync(id, mealEntry);
            mockMealEntryLogic.MockUpdateAsync(mealEntry);
            SetUpController(mealEntry.EntryUser.Id.ToString(), UserRole.Regular);
            var jsonDoc = new PatchMealEntryDTO { Text = "Good food", Calories = calories };

            //Act
            IActionResult response = await mealsController.PatchMealEntry(jsonDoc, id);

            //Assert
            ValidatePatchMealEntryTest(id, response, calories);
        }


        [Theory]
        [InlineData(0)]
        [InlineData(100.4)]
        public async Task PatchMealEntry_ValidAdmin_Returns200(double calories)
        {
            ///Arrange
            long id = mealEntry.Id;
            mockMealEntryLogic.MockGetAsync(id, mealEntry);
            mockMealEntryLogic.MockUpdateAsync(mealEntry);
            SetUpController("4", UserRole.Admin);
            var jsonDoc = new PatchMealEntryDTO { Text = "Good food", Calories = calories };

            //Act
            IActionResult response = await mealsController.PatchMealEntry(jsonDoc, id);

            //Assert
            ValidatePatchMealEntryTest(id, response, calories);
        }

        [Fact]
        public async Task PatchMealEntry_HandleFailureException_Returns500()
        {
            //Arrange
            long id = 2;
            mockMealEntryLogic.MockGetAsync(id, mealEntry);
            mockMealEntryLogic.MockUpdateAsync(mealEntry, new FailureException("New Error"));
            SetUpController("4", UserRole.Admin);
            var jsonDoc = new PatchMealEntryDTO { Text = "Good food" };

            //Act
            IActionResult response = await mealsController.PatchMealEntry(jsonDoc, id);

            //Assert
            ValidateApiError<ObjectResult>(response, 500, "New Error");

            RunVerifications();
        }

        [Fact]
        public async Task DeleteMealEntry_InvalidId_Returns404()
        {
            //Assert
            long id = 2;
            mockMealEntryLogic.MockGetAsync(id, null);
            //Act
            IActionResult response = await mealsController.DeleteMealEntry(id);

            //Assert
            ValidateApiError<NotFoundObjectResult>(response, 404, "Meal Entry Not Found");

            RunVerifications();
        }

        [Fact]
        public async Task DeleteMealEntry_Forbidden_Returns403()
        {
            //Arrange
            long id = 2;
            mockMealEntryLogic.MockGetAsync(id, new MealEntry { EntryUserId = 4 });
            SetUpController("3", UserRole.Regular);

            //Act
            IActionResult response = await mealsController.DeleteMealEntry(id);

            //Assert
            ValidateApiError<ObjectResult>(response, 403, "Forbidden");

            RunVerifications();
        }

        [Fact]
        public async Task DeleteMealEntry_Valid_Returns200()
        {
            //Arrange
            long id = 2;
            var jogEntry = new MealEntry { Id = 2, EntryUserId = 4 };
            mockMealEntryLogic.MockGetAsync(id, jogEntry);
            mockMealEntryLogic.MockDeleteAsync(id);
            SetUpController("4", UserRole.Regular);

            //Act
            IActionResult response = await mealsController.DeleteMealEntry(id);

            //Assert
            OkObjectResult result = response as OkObjectResult;
            var resultValue = result.Value as ApiResponse;

            Assert.NotNull(resultValue);
            Assert.True(resultValue.IsSuccessful);
            Assert.Equal("Successful", resultValue.Message);
            Assert.Equal(200, result.StatusCode);

            RunVerifications();
        }

        [Fact]
        public async Task DeleteMealEntry_HandleFailureException_Returns500()
        {
            //Arrange
            long id = 2;
            var jogEntry = new MealEntry { EntryUserId = 4 };
            mockMealEntryLogic.MockGetAsync(id, jogEntry);
            mockMealEntryLogic.MockDeleteAsync(id, new FailureException("New Error"));
            SetUpController("4", UserRole.Admin);

            //Act
            IActionResult response = await mealsController.DeleteMealEntry(id);

            //Assert
            ValidateApiError<ObjectResult>(response, 500, "New Error");

            RunVerifications();
        }

        [Fact]
        public async Task GetMealEntry_InvalidId_Returns404()
        {
            //Assert
            long id = 2;
            mockMealEntryLogic.MockGetAsync(id, null);
            //Act
            IActionResult response = await mealsController.GetMealEntry(id);

            //Assert
            ValidateApiError<NotFoundObjectResult>(response, 404, "Meal Entry Not Found");

            RunVerifications();
        }

        [Fact]
        public async Task GetMealEntry_Forbidden_Returns403()
        {
            //Arrange
            long id = 2;
            mockMealEntryLogic.MockGetAsync(id, new MealEntry { EntryUserId = 4 });
            SetUpController("3", UserRole.Regular);

            //Act
            IActionResult response = await mealsController.GetMealEntry(id);

            //Assert
            ValidateApiError<ObjectResult>(response, 403, "Forbidden");

            RunVerifications();
        }

        [Fact]
        public async Task GetMealEntry_Valid_Returns200()
        {
            //Arrange
            long id = 2;
            string mealEntryString = JsonConvert.SerializeObject(mealEntry);
            mockMealEntryLogic.MockGetAsync(id, mealEntry);
            SetUpController(mealEntry.EntryUserId.ToString(), UserRole.Regular);

            //Act
            IActionResult response = await mealsController.GetMealEntry(id);

            //Assert
            OkObjectResult result = response as OkObjectResult;
            var resultValue = result.Value as ApiObjectResponse<MealEntry>;

            Assert.NotNull(resultValue);
            Assert.NotNull(resultValue.Data);
            Assert.True(resultValue.IsSuccessful);
            Assert.Equal("Successful", resultValue.Message);
            Assert.Equal(200, result.StatusCode);

            Assert.Equal(mealEntryString, JsonConvert.SerializeObject(resultValue.Data));

            RunVerifications();
        }

        [Fact]
        public async Task GetMealEntry_HandleFailureException_Returns500()
        {
            //Arrange
            long id = 2;
            var jogEntry = new MealEntry { EntryUserId = 4 };
            mockMealEntryLogic.MockGetAsync(id, jogEntry, new FailureException("New Error"));
            SetUpController("4", UserRole.Admin);

            //Act
            IActionResult response = await mealsController.GetMealEntry(id);

            //Assert
            ValidateApiError<ObjectResult>(response, 500, "New Error");

            RunVerifications();
        }

        [Fact]
        public async Task SearchMealEntries_ValidAdmin_ReturnsEntries()
        {
            //arrange
            filter = new SearchFilter();
            SetUpSearchMealEntriesTest(filterString, filter);

            SetUpController("4", UserRole.Admin);

            mockMealEntryLogic.MockSearchEntriesAsync(filter, 1, 2, returnThis);

            //Act
            IActionResult response = await mealsController.SearchMealEntries(filterString, 1, 2);

            //Assert
            ValidateSearchMealEntriesTest(entrysCheck, response);
            Assert.Null(filter.LeftHandFilter);
            Assert.Null(filter.RightHandFilter);
        }

        [Theory]
        [InlineData("")]
        [InlineData(filterString)]
        public async Task SearchMealEntries_Valid_ReturnsEntries(string testFilterString)
        {
            //arrange
            SearchFilter testFilter = string.IsNullOrEmpty(testFilterString) ? null : new SearchFilter();
            SetUpSearchMealEntriesTest(testFilterString, testFilter);
            SetUpController("4", UserRole.Regular);

            mockMealEntryLogic.MockSearchEntriesWithAnyFilterAsync(1, 2, returnThis);

            //Act
            IActionResult response = await mealsController.SearchMealEntries(testFilterString, 1, 2);

            //Assert
            ValidateSearchMealEntriesTest(entrysCheck, response);
        }

        [Fact]
        public async Task SearchMealEntries_HandleFailureException_Returns500()
        {
            //Arrange
            SearchFilter filter = new SearchFilter();

            SetUpController("4", UserRole.Admin);

            mockSearchFilterHandler.MockParse<MealEntry>(filterString, filter, true);

            //Act
            IActionResult response = await mealsController.SearchMealEntries(filterString, 1, 2);

            //Assert
            ValidateApiError<ObjectResult>(response, 400, "Invalid Syntax at ()");

            RunVerifications();
        }

        private void SetUpController(string userId, UserRole role)
        {
            mealsController.ControllerContext.HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal()
            };
            var identity = new ClaimsIdentity();
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userId));
            identity.AddClaim(new Claim(ClaimTypes.Role, role.ToString()));
            mealsController.ControllerContext.HttpContext.User.AddIdentity(identity);
        }

        private void ValidateAddMealEntryTest(CreateMealEntryDTO entry, IActionResult response, double calories)
        {
            CreatedResult result = response as CreatedResult;
            var resultValue = result.Value as ApiObjectResponse<MealEntry>;

            Assert.NotNull(resultValue);
            Assert.True(resultValue.IsSuccessful);
            Assert.Equal("Successful", resultValue.Message);
            Assert.Equal(201, result.StatusCode);
            Assert.Equal("api/mealEntry/0", result.Location);

            Assert.Equal(2, resultValue.Data.EntryCreatorId);
            Assert.Equal(2, resultValue.Data.EntryUserId);
            Assert.Equal(entry.Calories, resultValue.Data.Calories);
            Assert.Equal(entry.EntryDateTime, resultValue.Data.EntryDateTime);
            Assert.Equal(entry.Text, resultValue.Data.Text);

            if (calories == 0)
            {
                Assert.Equal(CaloriesStatus.Pending, resultValue.Data.CaloriesStatus);
            }
            else
            {
                Assert.Equal(CaloriesStatus.CustomerProvided, resultValue.Data.CaloriesStatus);
            }

            RunVerifications();
        }
        private void ValidatePatchMealEntryTest(long id, IActionResult response, double calories)
        {
            OkObjectResult result = response as OkObjectResult;
            var resultValue = result.Value as ApiObjectResponse<MealEntry>;

            Assert.NotNull(resultValue);
            Assert.True(resultValue.IsSuccessful);
            Assert.Equal("Successful", resultValue.Message);
            Assert.Equal(200, result.StatusCode);

            Assert.NotNull(resultValue.Data);
            Assert.Equal(mealEntry.EntryCreatorId, resultValue.Data.EntryCreatorId);
            Assert.Equal(mealEntry.EntryUserId, resultValue.Data.EntryUserId);
            Assert.Equal(mealEntry.Calories, resultValue.Data.Calories);
            Assert.Equal(mealEntry.EntryDateTime, resultValue.Data.EntryDateTime);
            Assert.Equal(mealEntry.Text, resultValue.Data.Text);
            Assert.Equal(id, resultValue.Data.Id);

            if (calories == 0)
            {
                Assert.Equal(CaloriesStatus.Pending, resultValue.Data.CaloriesStatus);
            }
            else
            {
                Assert.Equal(CaloriesStatus.CustomerProvided, resultValue.Data.CaloriesStatus);
            }

            RunVerifications();
        }

        private static CreateMealEntryDTO ConvertToDTO(MealEntry entry)
        {
            return new CreateMealEntryDTO
            {
                Calories = entry.Calories,
                EntryDateTime = entry.EntryDateTime,
                EntryUser = new EntryUserDTO { Id = entry.EntryUserId },
                Text = entry.Text
            };
        }

        private void SetUpSearchMealEntriesTest(string testFilterString, SearchFilter filter)
        {
            List<MealEntry> entrys = MealEntryUtil.GenerateEntries(currentTime, 4);
            entrysCheck = MealEntryUtil.GenerateEntries(currentTime, 4);
            returnThis = new SearchResult<MealEntry> { Results = entrys, TotalCount = 20 };
            mockSearchFilterHandler.MockParse<MealEntry>(testFilterString, filter);
        }

        private void ValidateSearchMealEntriesTest(List<MealEntry> entrysCheck, IActionResult response)
        {
            OkObjectResult result = response as OkObjectResult;
            var resultValue = result.Value as ApiObjectResponse<SearchResult<MealEntry>>;

            Assert.NotNull(resultValue);
            Assert.NotNull(resultValue.Data);
            Assert.True(resultValue.IsSuccessful);
            Assert.Equal("Successful", resultValue.Message);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal(20, resultValue.Data.TotalCount);

            var mealEntryVer = MealEntryUtil.GenerateVerifications(entrysCheck);
            Assert.Collection(resultValue.Data.Results, mealEntryVer);

            RunVerifications();
        }

        private void RunVerifications()
        {
            mockMealEntryLogic.RunVerification();
            mockSearchFilterHandler.RunVerification();
            mockUserLogic.RunVerification();
        }
    }
}
