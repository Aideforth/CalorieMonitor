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
using Microsoft.Extensions.Configuration;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace CalorieMonitor.UnitTests.Tests.Controllers
{
    public class UsersControllerTest : ControllerTestBase
    {
        readonly MockUserLogic mockUserLogic;
        readonly MockSearchFilterHandler mockSearchFilterHandler;
        readonly UsersController usersController;
        readonly DateTime currentTime;
        readonly User user;
        readonly User userCheck;

        public UsersControllerTest()
        {
            mockUserLogic = new MockUserLogic();
            mockSearchFilterHandler = new MockSearchFilterHandler();

            usersController = new UsersController(mockUserLogic.Object, mockSearchFilterHandler.Object);
            currentTime = DateTime.UtcNow;
            user = UserUtil.GenerateRecords(currentTime, 1)[0];
            userCheck = UserUtil.GenerateRecords(currentTime, 1)[0];
        }

        [Fact]
        public void Constructor_NullIUserLogicArgument_ThrowsException()
        {
            Exception exception = Record.Exception(() => new UsersController(null, mockSearchFilterHandler.Object));
            Assert.Equal("userLogic", (exception as ArgumentNullException).ParamName);
        }

        [Fact]
        public void Constructor_NullISearchFilterHandlerArgument_ThrowsException()
        {
            Exception exception = Record.Exception(() => new UsersController(mockUserLogic.Object, null));
            Assert.IsType<ArgumentNullException>(exception);
            Assert.Equal("searchFilterHandler", (exception as ArgumentNullException).ParamName);
        }

        [Fact]
        public async Task CreateUser_NullBody_Returns400()
        {
            //Act
            IActionResult response = await usersController.CreateUser(null);

            //Assert
            ValidateBadRequest(response, "Result", "No Payload was sent");

            RunVerifications();
        }

        [Fact]
        public async Task CreateUser_InvalidModel_Returns400()
        {
            //Arrange
            usersController.ModelState.AddModelError("Test", "Test Error");

            //Act
            IActionResult response = await usersController.CreateUser(new CreateUserDTO());

            //Assert
            ValidateBadRequest(response, "Test", "Test Error");

            RunVerifications();
        }

        [Fact]
        public async Task CreateUser_Valid_Returns201()
        {
            //Arrange
            user.Role = UserRole.Regular;
            userCheck.Role = UserRole.Regular;
            mockUserLogic.MockCreateAsync();

            CreateUserDTO userDTO = ConvertToDTO(user);
            //Act
            IActionResult response = await usersController.CreateUser(userDTO);

            //Assert
            ValidateCreateUser(response);
        }

        [Theory]
        [InlineData(UserRole.Admin)]
        [InlineData(UserRole.UserManager)]
        public async Task CreateUser_ValidAdmin_Returns201(UserRole role)
        {
            //Arrange
            SetUpController("2", UserRole.Admin);
            user.Role = role;
            userCheck.Role = role;
            mockUserLogic.MockCreateAsync();

            CreateUserDTO userDTO = ConvertToDTO(user);

            //Act
            IActionResult response = await usersController.CreateUser(userDTO);

            //Assert
            ValidateCreateUser(response);
        }

        [Fact]
        public async Task CreateUser_HandleBusinessException_Returns400()
        {
            //Arrange
            CreateUserDTO user = new CreateUserDTO { };
            mockUserLogic.MockCreateAsync(new BusinessException("New Error"));

            //Act
            IActionResult response = await usersController.CreateUser(user);

            //Assert
            ValidateApiError<ObjectResult>(response, 400, "New Error");
            RunVerifications();
        }
        [Fact]
        public async Task CreateUser_HandleFailureException_Returns500()
        {
            //Arrange
            CreateUserDTO user = new CreateUserDTO { };
            mockUserLogic.MockCreateAsync(new FailureException("New Error"));

            //Act
            IActionResult response = await usersController.CreateUser(user);

            //Assert
            ValidateApiError<ObjectResult>(response, 500, "New Error");
            RunVerifications();
        }

        [Fact]
        public async Task CreateUser_HandleOtherException_Returns500()
        {
            //Arrange
            CreateUserDTO user = new CreateUserDTO { };
            mockUserLogic.MockCreateAsync(new Exception("New Error"));

            //Act
            IActionResult response = await usersController.CreateUser(user);

            //Assert
            ValidateApiError<ObjectResult>(response, 500, "An error occurred, please try again");
            RunVerifications();
        }

        [Fact]
        public async Task PatchUser_InvalidId_Returns404()
        {
            //Assert
            long id = 2;
            mockUserLogic.MockGetAsync(id, null);
            //Act
            IActionResult response = await usersController.PatchUser(null, id);

            //Assert
            ValidateApiError<NotFoundObjectResult>(response, 404, "User Not Found");

            RunVerifications();
        }

        [Theory]
        [InlineData(UserRole.Regular, UserRole.Regular)]
        [InlineData(UserRole.Regular, UserRole.Admin)]
        [InlineData(UserRole.Regular, UserRole.UserManager)]
        [InlineData(UserRole.UserManager, UserRole.Admin)]
        [InlineData(UserRole.UserManager, UserRole.UserManager)]
        public async Task PatchUser_Forbidden_Returns403(UserRole userRole, UserRole roleToUpdate)
        {
            //Arrange
            long id = 2;
            mockUserLogic.MockGetAsync(id, new User { Id = 4, Role = roleToUpdate });
            SetUpController("3", userRole);

            //Act
            IActionResult response = await usersController.PatchUser(null, id);

            //Assert
            ValidateApiError<ObjectResult>(response, 403, "Forbidden");

            RunVerifications();
        }

        [Theory]
        [InlineData(UserRole.UserManager, UserRole.Admin)]
        [InlineData(UserRole.UserManager, UserRole.UserManager)]
        public async Task PatchUser_UnAuthorizedChangeRole_Returns401(UserRole userRole, UserRole updateRole)
        {
            //Arrange
            long id = 2;
            mockUserLogic.MockGetAsync(id, new User { Id = 4 });
            SetUpController("3", userRole);
            var jsonDoc = new PatchUserDTO { Role = updateRole };

            //Act
            IActionResult response = await usersController.PatchUser(jsonDoc, id);

            //Assert
            ValidateApiError<ObjectResult>(response, 400, "Only Admins are allowed to change user role");

            RunVerifications();
        }

        [Fact]
        public async Task PatchUser_NullBody_Returns400()
        {
            //Arrange
            long id = 2;
            mockUserLogic.MockGetAsync(id, new User { Id = 4 });
            SetUpController("4", UserRole.Regular);

            //Act
            IActionResult response = await usersController.PatchUser(null, 2);

            //Assert
            ValidateBadRequest(response, "Result", "No Payload was sent");

            RunVerifications();
        }

        [Fact]
        public async Task PatchUser_InvalidModel_Returns400()
        {
            //Arrange
            long id = 2;
            mockUserLogic.MockGetAsync(id, new User { Id = 4 });
            SetUpController("4", UserRole.Regular);
            usersController.ModelState.AddModelError("Test", "Test Error");

            //Act
            IActionResult response = await usersController.PatchUser(new PatchUserDTO(), 2);

            //Assert
            ValidateBadRequest(response, "Test", "Test Error");

            RunVerifications();
        }

        [Fact]
        public async Task PatchUser_Valid_Returns200()
        {
            //Arrange
            long id = 2;
            PatchUserDTO jsonDoc = SetUpPatchUserTest(id);

            //Act
            IActionResult response = await usersController.PatchUser(jsonDoc, id);

            //Assert
            ValidatePatchUserTest(response);
        }

        [Fact]
        public async Task PatchSelf_Valid_Returns200()
        {
            //Arrange
            SetUpController("1", user.Role);
            PatchUserDTO jsonDoc = SetUpPatchUserTest(1);

            //Act
            IActionResult response = await usersController.PatchSelf(jsonDoc);

            //Assert
            ValidatePatchUserTest(response);
        }

        [Theory]
        [InlineData(UserRole.UserManager, UserRole.Regular)]
        [InlineData(UserRole.Admin, UserRole.Admin)]
        [InlineData(UserRole.Admin, UserRole.UserManager)]
        [InlineData(UserRole.Admin, UserRole.Regular)]
        public async Task PatchUser_ValidUser_Returns200(UserRole userRole, UserRole updateRole)
        {
            ///Arrange
            long id = 1;
            mockUserLogic.MockGetAsync(id, user);
            mockUserLogic.MockUpdateAsync();
            SetUpController("4", userRole);
            PatchUserDTO jsonDoc = new PatchUserDTO { Role = updateRole };
            user.Role = UserRole.Regular;
            userCheck.Role = updateRole;

            //Act
            IActionResult response = await usersController.PatchUser(jsonDoc, id);

            //Assert
            ValidatePatchUserTest(response);
        }

        [Fact]
        public async Task PatchUser_HandleFailureException_Returns500()
        {
            //Arrange
            long id = 2;
            var user = new User { Id = 2 };
            mockUserLogic.MockGetAsync(id, user);
            mockUserLogic.MockUpdateAsync(new FailureException("New Error"));
            SetUpController("4", UserRole.Admin);
            PatchUserDTO jsonDoc = new PatchUserDTO();

            //Act
            IActionResult response = await usersController.PatchUser(jsonDoc, id);

            //Assert
            ValidateApiError<ObjectResult>(response, 500, "New Error");

            RunVerifications();
        }

        [Fact]
        public async Task DeleteUser_InvalidId_Returns404()
        {
            //Assert
            long id = 2;
            mockUserLogic.MockGetAsync(id, null);
            //Act
            IActionResult response = await usersController.DeleteUser(id);

            //Assert
            ValidateApiError<NotFoundObjectResult>(response, 404, "User Not Found");

            RunVerifications();
        }

        [Fact]
        public async Task DeleteUser_Forbidden_Returns401()
        {
            //Arrange
            long id = 2;
            mockUserLogic.MockGetAsync(id, new User { Id = 4 });
            SetUpController("3", UserRole.Regular);

            //Act
            IActionResult response = await usersController.DeleteUser(id);

            //Assert
            ValidateApiError<ObjectResult>(response, 403, "Forbidden");

            RunVerifications();
        }

        [Fact]
        public async Task DeleteUser_Valid_Returns200()
        {
            //Arrange
            long id = 4;
            var user = new User { Id = 4 };
            mockUserLogic.MockGetAsync(id, user);
            mockUserLogic.MockDeleteAsync(id);
            SetUpController("4", UserRole.Regular);

            //Act
            IActionResult response = await usersController.DeleteUser(id);

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
        public async Task DeleteUser_HandleFailureException_Returns500()
        {
            //Arrange
            long id = 4;
            var user = new User { Id = 4 };
            mockUserLogic.MockGetAsync(id, user);
            mockUserLogic.MockDeleteAsync(id, new FailureException("New Error"));
            SetUpController("4", UserRole.Admin);

            //Act
            IActionResult response = await usersController.DeleteUser(id);

            //Assert
            ValidateApiError<ObjectResult>(response, 500, "New Error");

            RunVerifications();
        }

        [Fact]
        public async Task GetUser_InvalidId_Returns404()
        {
            //Assert
            long id = 2;
            mockUserLogic.MockGetAsync(id, null);
            //Act
            IActionResult response = await usersController.GetUser(id);

            //Assert
            ValidateApiError<NotFoundObjectResult>(response, 404, "User Not Found");

            RunVerifications();
        }

        [Fact]
        public async Task GetUser_Forbidden_Returns403()
        {
            //Arrange
            long id = 4;
            mockUserLogic.MockGetAsync(id, new User { Id = 4 });
            SetUpController("3", UserRole.Regular);

            //Act
            IActionResult response = await usersController.GetUser(id);

            //Assert
            ValidateApiError<ObjectResult>(response, 403, "Forbidden");

            RunVerifications();
        }

        [Fact]
        public async Task GetUser_Valid_Returns200()
        {
            //Arrange
            long id = 4;
            User user = SetUpGetUserTest(id);

            //Act
            IActionResult response = await usersController.GetUser(id);

            //Assert
            ValidateGetUserTest(user, response);
        }

        [Fact]
        public async Task GetSelf_Valid_Returns200()
        {
            //Arrange
            long id = 4;
            User user = SetUpGetUserTest(id);

            //Act
            IActionResult response = await usersController.GetSelf();

            //Assert
            ValidateGetUserTest(user, response);
        }

        [Fact]
        public async Task GetUser_HandleFailureException_Returns500()
        {
            //Arrange
            long id = 4;
            var user = new User { Id = 4 };
            mockUserLogic.MockGetAsync(id, user, new FailureException("New Error"));
            SetUpController("4", UserRole.Admin);

            //Act
            IActionResult response = await usersController.GetUser(id);

            //Assert
            ValidateApiError<ObjectResult>(response, 500, "New Error");

            RunVerifications();
        }

        [Fact]
        public async Task SearchUsers_Valid_ReturnsUsers()
        {
            //arrange
            SearchFilter filter = new SearchFilter();
            List<User> Users = UserUtil.GenerateRecords(currentTime, 4);
            List<User> UsersCheck = UserUtil.GenerateRecords(currentTime, 4);
            SearchResult<User> returnThis = new SearchResult<User> { Results = Users, TotalCount = 20 };
            string fllterString = "(UserDate eq '2019-02-12')";

            mockUserLogic.MockSearchUsersAsync(filter, 1, 2, returnThis);
            mockSearchFilterHandler.MockParse<User>(fllterString, filter);

            //Act
            IActionResult response = await usersController.SearchUsers(fllterString, 1, 2);

            OkObjectResult result = response as OkObjectResult;
            var resultValue = result.Value as ApiObjectResponse<SearchResult<User>>;

            Assert.NotNull(resultValue);
            Assert.NotNull(resultValue.Data);
            Assert.True(resultValue.IsSuccessful);
            Assert.Equal("Successful", resultValue.Message);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal(20, resultValue.Data.TotalCount);

            var UserVer = UserUtil.GenerateVerifications(UsersCheck);
            Assert.Collection(resultValue.Data.Results, UserVer);

            RunVerifications();
        }

        [Fact]
        public async Task SearchUsers_HandleFailureException_Returns500()
        {
            //Arrange
            SearchFilter filter = new SearchFilter();
            string fllterString = "(UserDate eq '2019-02-12')";

            mockSearchFilterHandler.MockParse<User>(fllterString, filter, true);

            //Act
            IActionResult response = await usersController.SearchUsers(fllterString, 1, 2);

            //Assert
            ValidateApiError<ObjectResult>(response, 400, "Invalid Syntax at ()");

            RunVerifications();
        }

        [Fact]
        public async Task LoginUser_Valid_GetsToken()
        {
            //Arrange
            string token = "token";
            mockUserLogic.MockLoginUserAsync(user.UserName, user.Password, token);

            //Act
            IActionResult response = await usersController.LoginUser(new LoginRequest { UserName = user.UserName, Password = user.Password });

            //Assert
            OkObjectResult result = response as OkObjectResult;
            var resultValue = result.Value as ApiObjectResponse<TokenInfo>;

            Assert.NotNull(resultValue);
            Assert.True(resultValue.IsSuccessful);
            Assert.Equal("Successful", resultValue.Message);
            Assert.Equal(200, result.StatusCode);

            Assert.NotNull(resultValue.Data);
            Assert.NotNull(resultValue.Data.Token);
            Assert.Equal(token, resultValue.Data.Token);

            RunVerifications();
        }

        [Fact]
        public async Task LoginUser_NullUser_UnAuthorizedResponse()
        {
            //Arrange
            mockUserLogic.MockLoginUserAsync(user.UserName, user.Password, null, new UnauthorizedException());

            //Act
            IActionResult response = await usersController.LoginUser(new LoginRequest { UserName = user.UserName, Password = user.Password });

            //Assert
            ValidateApiError<UnauthorizedObjectResult>(response, 401, "Unauthorized");

            RunVerifications();
        }
        [Fact]
        public async Task ChangePassword_Valid_Succeeds()
        {
            //Arrange
            string newPassword = "new-password";
            SetUpController(user.Id.ToString(), user.Role);
            mockUserLogic.MockChangeUserPasswordAsync(user.Id, user.Password, newPassword);

            //Act
            IActionResult response = await usersController.ChangePassword(new ChangePasswordRequest
            {
                OldPassword = user.Password,
                NewPassword = newPassword
            });

            //Assert
            OkObjectResult result = response as OkObjectResult;
            Assert.NotNull(result);
            var resultValue = result.Value as ApiResponse;

            Assert.NotNull(resultValue);
            Assert.True(resultValue.IsSuccessful);
            Assert.Equal("Successful", resultValue.Message);
            Assert.Equal(200, result.StatusCode);

            RunVerifications();
        }

        [Fact]
        public async Task ChangePassword_NullUser_UnAuthorisedResponse()
        {
            //Arrange
            SetUpController(user.Id.ToString(), user.Role);
            mockUserLogic.MockChangeUserPasswordAsync(user.Id, user.Password, null, new UnauthorizedException());

            //Act
            IActionResult response = await usersController.ChangePassword(new ChangePasswordRequest
            {
                OldPassword = user.Password,
                NewPassword = null
            });

            //Assert
            ValidateApiError<UnauthorizedObjectResult>(response, 401, "Unauthorized");

            RunVerifications();
        }

        private static CreateUserDTO ConvertToDTO(User user)
        {
            return new CreateUserDTO
            {
                EmailAddress = user.EmailAddress,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Password = user.Password,
                UserName = user.UserName,
                Role = user.Role,
                DailyCalorieLimit = user.DailyCalorieLimit
            };
        }

        private void ValidateCreateUser(IActionResult response)
        {
            CreatedResult result = response as CreatedResult;
            var resultValue = result.Value as ApiObjectResponse<User>;
            Assert.NotNull(resultValue);
            Assert.True(resultValue.IsSuccessful);
            Assert.Equal("Successful", resultValue.Message);
            Assert.Equal(201, result.StatusCode);
            Assert.Equal($"api/users/{resultValue.Data.Id}", result.Location);

            Assert.Equal(resultValue.Data.EmailAddress, userCheck.EmailAddress);
            Assert.Equal(resultValue.Data.FirstName, userCheck.FirstName);
            Assert.Equal(resultValue.Data.LastName, userCheck.LastName);
            Assert.Equal(resultValue.Data.Password, userCheck.Password);
            Assert.Equal(resultValue.Data.UserName, userCheck.UserName);
            Assert.Equal(resultValue.Data.Role, userCheck.Role);
            Assert.Equal(resultValue.Data.DailyCalorieLimit, userCheck.DailyCalorieLimit);
            RunVerifications();
        }

        private PatchUserDTO SetUpPatchUserTest(long id)
        {
            mockUserLogic.MockGetAsync(id, user);
            mockUserLogic.MockUpdateAsync();
            SetUpController("1", UserRole.Regular);
            PatchUserDTO jsonDoc = new PatchUserDTO { FirstName = "NewName Now" };
            userCheck.FirstName = "NewName Now";
            return jsonDoc;
        }

        private void SetUpController(string userId, UserRole role)
        {
            usersController.ControllerContext.HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal()
            };
            var identity = new ClaimsIdentity();
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userId));
            identity.AddClaim(new Claim(ClaimTypes.Role, role.ToString()));
            usersController.ControllerContext.HttpContext.User.AddIdentity(identity);
        }

        private void ValidatePatchUserTest(IActionResult response)
        {
            OkObjectResult result = response as OkObjectResult;
            var resultValue = result.Value as ApiObjectResponse<User>;

            Assert.NotNull(resultValue);
            Assert.True(resultValue.IsSuccessful);
            Assert.Equal("Successful", resultValue.Message);
            Assert.Equal(200, result.StatusCode);

            Assert.NotNull(resultValue.Data);
            var userVer = UserUtil.GenerateVerifications(new List<User> { userCheck })[0];
            userVer(resultValue.Data);

            RunVerifications();
        }

        private User SetUpGetUserTest(long id)
        {
            var user = new User { Id = 4 };
            mockUserLogic.MockGetAsync(id, user);
            SetUpController("4", UserRole.Regular);
            return user;
        }

        private void ValidateGetUserTest(User user, IActionResult response)
        {
            OkObjectResult result = response as OkObjectResult;
            var resultValue = result.Value as ApiObjectResponse<User>;

            Assert.NotNull(resultValue);
            Assert.NotNull(resultValue.Data);
            Assert.True(resultValue.IsSuccessful);
            Assert.Equal("Successful", resultValue.Message);
            Assert.Equal(200, result.StatusCode);

            string UserString = JsonConvert.SerializeObject(user);
            Assert.Equal(UserString, JsonConvert.SerializeObject(resultValue.Data));

            RunVerifications();
        }

        private void RunVerifications()
        {
            mockUserLogic.RunVerification();
            mockSearchFilterHandler.RunVerification();
        }
    }
}
