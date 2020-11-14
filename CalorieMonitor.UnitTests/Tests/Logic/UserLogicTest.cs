using CalorieMonitor.Core.Entities;
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
    public class UserLogicTest
    {
        readonly MockUserDAO mockUserDAO;
        readonly MockLogManager mockLogManager;
        readonly MockPasswordHashProvider mockPasswordHashProvider;
        readonly MockLoginHandler mockLoginHandler;
        readonly UserLogic userLogic;
        readonly DateTime currentTime;
        const string userName = "userName";
        const string password = "password";
        const string newPassword = "newPassword";
        const string hashedPassword = "HashedPassword";
        const string hashedNewPassword = "HashedNewPassword";
        readonly User user;
        readonly User userCheck;

        public UserLogicTest()
        {
            mockUserDAO = new MockUserDAO();
            mockLogManager = new MockLogManager();
            mockPasswordHashProvider = new MockPasswordHashProvider();
            mockLoginHandler = new MockLoginHandler();

            userLogic = new UserLogic(mockUserDAO.Object, mockPasswordHashProvider.Object, mockLoginHandler.Object, mockLogManager.Object);
            currentTime = DateTime.UtcNow;
            user = UserUtil.GenerateRecords(currentTime, 1)[0];
            userCheck = UserUtil.GenerateRecords(currentTime, 1)[0];
        }

        [Fact]
        public void Constructor_NullIUserDAOArgument_ThrowsException()
        {
            Exception exception = Record.Exception(() => new UserLogic(null, mockPasswordHashProvider.Object, mockLoginHandler.Object, mockLogManager.Object));
            Assert.IsType<ArgumentNullException>(exception);
            Assert.Equal("userDAO", (exception as ArgumentNullException).ParamName);
        }

        [Fact]
        public void Constructor_NullIPasswordHashProviderArgument_ThrowsException()
        {
            Exception exception = Record.Exception(() => new UserLogic(mockUserDAO.Object, null, mockLoginHandler.Object, mockLogManager.Object));
            Assert.IsType<ArgumentNullException>(exception);
            Assert.Equal("hashProvider", (exception as ArgumentNullException).ParamName);
        }

        [Fact]
        public void Constructor_NullILoginHandlerArgument_ThrowsException()
        {
            Exception exception = Record.Exception(() => new UserLogic(mockUserDAO.Object, mockPasswordHashProvider.Object, null, mockLogManager.Object));
            Assert.IsType<ArgumentNullException>(exception);
            Assert.Equal("loginHandler", (exception as ArgumentNullException).ParamName);
        }

        [Fact]
        public void Constructor_NullILogManagerArgument_ThrowsException()
        {
            Exception exception = Record.Exception(() => new UserLogic(mockUserDAO.Object, mockPasswordHashProvider.Object, mockLoginHandler.Object, null));
            Assert.IsType<ArgumentNullException>(exception);
            Assert.Equal("logManager", (exception as ArgumentNullException).ParamName);
        }

        [Fact]
        public async Task CreateAsync_Valid_Returnsuser()
        {
            //arrange
            SetUpUserNameAndEmailUniqueChecks(null, null);
            mockUserDAO.MockInsertAsync(user);
            mockPasswordHashProvider.MockComputeHash(user.Password, hashedPassword);
            userCheck.Password = hashedPassword;

            //Act
            User response = await userLogic.CreateAsync(user);

            //Assert
            Assert.NotNull(response);
            Assert.Same(user, response);
            Assert.True(currentTime < response.DateCreated);

            //validate users with new date
            userCheck.DateCreated = response.DateCreated;

            var UserVer = UserUtil.GenerateVerifications(new List<User> { userCheck })[0];
            UserVer(response);

            RunVerification();
        }

        [Fact]
        public async Task CreateAsync_DuplicateEmail_ThrowsFailureException()
        {
            //arrange
            mockUserDAO.MockGetUserByEmailAsync(user.EmailAddress, user);
            mockLogManager.MockLogException<BusinessException>();

            //Act
            Exception exception = await Record.ExceptionAsync(() => userLogic.CreateAsync(user));

            //Assert
            Assert.IsType<BusinessException>(exception);
            Assert.Equal("The email provided is already registered", exception.Message);

            RunVerification();
        }

        [Fact]
        public async Task CreateAsync_DuplicateUsername_ThrowsFailureException()
        {
            //arrange
            SetUpUserNameAndEmailUniqueChecks(null, user);
            mockLogManager.MockLogException<BusinessException>();

            //Act
            Exception exception = await Record.ExceptionAsync(() => userLogic.CreateAsync(user));

            //Assert
            Assert.IsType<BusinessException>(exception);
            Assert.Equal("The username provided is already registered", exception.Message);

            RunVerification();
        }

        [Fact]
        public async Task CreateAsync_Error_ThrowsFailureException()
        {
            //arrange
            SetUpUserNameAndEmailUniqueChecks(null, null);
            mockUserDAO.MockInsertAsync(user, true);
            mockLogManager.MockLogException<SqlException>();
            mockPasswordHashProvider.MockComputeHash(user.Password, hashedPassword);

            //Act
            Exception exception = await Record.ExceptionAsync(() => userLogic.CreateAsync(user));

            //Assert
            Assert.IsType<FailureException>(exception);
            Assert.Equal("An error occurred while saving the user, please try again", exception.Message);

            RunVerification();
        }
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task UpdateAsync_Valid_Returnsuser(bool existingEmailAndUsername)
        {
            //arrange
            User checkWithUser = existingEmailAndUsername ? user : null;
            SetUpUserNameAndEmailUniqueChecks(checkWithUser, checkWithUser);
            mockUserDAO.MockUpdateAsync(user);

            //Act
            User response = await userLogic.UpdateAsync(user);

            //Assert
            Assert.NotNull(response);
            Assert.Same(user, response);
            Assert.True(currentTime < response.DateUpdated);

            //validate users with new date
            userCheck.DateUpdated = response.DateUpdated;

            var UserVer = UserUtil.GenerateVerifications(new List<User> { userCheck })[0];
            UserVer(response);

            RunVerification();
        }
        [Fact]
        public async Task UpdateAsync_DuplicateEmail_ThrowsFailureException()
        {
            //arrange
            userCheck.Id = 3;

            mockUserDAO.MockGetUserByEmailAsync(user.EmailAddress, userCheck);
            mockLogManager.MockLogException<BusinessException>();

            //Act
            Exception exception = await Record.ExceptionAsync(() => userLogic.UpdateAsync(user));

            //Assert
            Assert.IsType<BusinessException>(exception);
            Assert.Equal("The email provided is already registered to another user", exception.Message);

            RunVerification();
        }

        [Fact]
        public async Task UpdateAsync_DuplicateUsername_ThrowsFailureException()
        {
            //arrange
            userCheck.Id = 3;

            SetUpUserNameAndEmailUniqueChecks(user, userCheck);
            mockLogManager.MockLogException<BusinessException>();

            //Act
            Exception exception = await Record.ExceptionAsync(() => userLogic.UpdateAsync(user));

            //Assert
            Assert.IsType<BusinessException>(exception);
            Assert.Equal("The username provided is already registered to another user", exception.Message);

            RunVerification();
        }

        [Fact]
        public async Task UpdateAsync_Error_ThrowsFailureException()
        {
            //arrange
            SetUpUserNameAndEmailUniqueChecks(user, user);
            mockUserDAO.MockUpdateAsync(user, true);
            mockLogManager.MockLogException<SqlException>();

            //Act
            Exception exception = await Record.ExceptionAsync(() => userLogic.UpdateAsync(user));

            //Assert
            Assert.IsType<FailureException>(exception);
            Assert.Equal("An error occurred while updating the user, please try again", exception.Message);

            RunVerification();
        }

        [Fact]
        public async Task GetAsync_Valid_Returnsuser()
        {
            //arrange
            long id = 2;
            mockUserDAO.MockGetAsync(id, user);

            //Act
            User response = await userLogic.GetAsync(id);

            //Assert
            Assert.NotNull(response);
            var UserVer = UserUtil.GenerateVerifications(new List<User> { userCheck })[0];
            UserVer(response);

            RunVerification();
        }
        [Fact]
        public async Task GetAsync_Valid_ReturnsNull()
        {
            //arrange
            long id = 2;
            mockUserDAO.MockGetAsync(id, null);

            //Act
            User response = await userLogic.GetAsync(id);

            //Assert
            Assert.Null(response);
            RunVerification();
        }

        [Fact]
        public async Task GetAsync_Error_ThrowsFailureException()
        {
            //arrange
            long id = 2;
            mockUserDAO.MockGetAsync(id, null, true);
            mockLogManager.MockLogException<SqlException>();

            //Act
            Exception exception = await Record.ExceptionAsync(() => userLogic.GetAsync(id));

            //Assert
            Assert.IsType<FailureException>(exception);
            Assert.Equal("An error occurred while retrieving the user, please try again", exception.Message);

            RunVerification();
        }

        [Fact]
        public async Task DeleteAsync_Valid_ReturnsTrue()
        {
            //arrange
            long id = 2;
            User user = UserUtil.GenerateRecords(currentTime, 1)[0];

            mockUserDAO.MockGetAsync(id, user);
            mockUserDAO.MockDeleteAsync(id);

            //Act
            bool response = await userLogic.DeleteAsync(id);

            //Assert
            Assert.True(response);

            RunVerification();
        }

        [Fact]
        public async Task DeleteAsync_userDoesNotExist_ThrowsNotFoundException()
        {
            //arrange
            long id = 2;
            mockUserDAO.MockGetAsync(id, null);
            mockLogManager.MockLogException<NotFoundException>();

            //Act
            Exception exception = await Record.ExceptionAsync(() => userLogic.DeleteAsync(id));

            //Assert
            Assert.IsType<NotFoundException>(exception);
            Assert.Equal("The user does not exist", exception.Message);

            RunVerification();
        }
        [Fact]
        public async Task DeleteAsync_Error_ThrowsFailureException()
        {
            //arrange
            long id = 2;
            mockUserDAO.MockGetAsync(id, null, true);
            mockLogManager.MockLogException<SqlException>();

            //Act
            Exception exception = await Record.ExceptionAsync(() => userLogic.DeleteAsync(id));

            //Assert
            Assert.IsType<FailureException>(exception);
            Assert.Equal("An error occurred while deleting the user, please try again", exception.Message);

            RunVerification();
        }

        [Fact]
        public async Task SearchUsersAsync_Valid_Returnsuser()
        {
            //arrange
            SearchFilter filter = new SearchFilter();
            List<User> users = UserUtil.GenerateRecords(currentTime, 4);
            List<User> usersCheck = UserUtil.GenerateRecords(currentTime, 4);
            SearchResult<User> returnThis = new SearchResult<User> { Results = users, TotalCount = 20 };

            mockUserDAO.MockSearchUsersAsync(filter, 1, 2, returnThis);

            //Act
            SearchResult<User> response = await userLogic.SearchUsersAsync(filter, 1, 2);

            //Assert
            Assert.NotNull(response);
            Assert.NotNull(response.Results);
            Assert.Equal(20, response.TotalCount);

            var UserVer = UserUtil.GenerateVerifications(usersCheck);
            Assert.Collection(response.Results, UserVer);

            RunVerification();
        }

        [Fact]
        public async Task SearchUsersAsync_Error_ThrowsFailureException()
        {
            //arrange
            SearchFilter filter = new SearchFilter();
            mockUserDAO.MockSearchUsersAsync(filter, 1, 2, null, true);
            mockLogManager.MockLogException<SqlException>();

            //Act
            Exception exception = await Record.ExceptionAsync(() => userLogic.SearchUsersAsync(filter, 1, 2));

            //Assert
            Assert.IsType<FailureException>(exception);
            Assert.Equal("An error occurred while retrieving the users, please try again", exception.Message);
            RunVerification();
        }

        [Fact]
        public async Task LoginUserAsync_Valid_ReturnsTrue()
        {
            //arrange
            user.Password = hashedPassword;
            string generatedToken = "token";
            SetUpLoginUserAsync(user);
            mockLoginHandler.MockGetCalorieResultAsync(user, generatedToken);

            //Act
            string token = await userLogic.LoginUserAsync(userName, password);

            //Assert
            Assert.NotNull(token);
            Assert.Equal(generatedToken, token);

            RunVerification();
        }

        [Fact]
        public async Task LoginUserAsync_InvalidPassword_ThrowsUnAuthorizedException()
        {
            //arrange
            SetUpLoginUserAsync(user);
            mockLogManager.MockLogMessage("Invalid Password");
            mockLogManager.MockLogException<UnauthorizedException>();

            //Act
            Exception exception = await Record.ExceptionAsync(() => userLogic.LoginUserAsync(userName, password));

            //Assert
            Assert.IsType<UnauthorizedException>(exception);

            RunVerification();
        }

        [Fact]
        public async Task LoginUserAsync_NullUser_ThrowsUnAuthorizedException()
        {
            //arrange
            mockUserDAO.MockGetUserByUserNameAsync(userName, null);
            mockLogManager.MockLogMessage("Invalid Username");
            mockLogManager.MockLogException<UnauthorizedException>();

            //Act
            Exception exception = await Record.ExceptionAsync(() => userLogic.LoginUserAsync(userName, null));

            //Assert
            Assert.IsType<UnauthorizedException>(exception);

            RunVerification();
        }

        [Fact]
        public async Task ChangeUserPasswordAsync_Valid_ReturnsTrue()
        {
            //arrange
            user.Password = hashedPassword;
            SetUpChangeUserPasswordAsync(user);
            mockPasswordHashProvider.MockComputeHash(newPassword, hashedNewPassword);
            mockUserDAO.MockUpdateAsync(user);

            //Act
            await userLogic.ChangeUserPasswordAsync(user.Id, password, newPassword);

            //Assert
            RunVerification();
        }

        [Fact]
        public async Task ChangeUserPasswordAsync_InvalidPassword_ThrowsUnAuthorizedException()
        {
            //arrange
            SetUpChangeUserPasswordAsync(user);
            mockLogManager.MockLogMessage("Invalid Password");
            mockLogManager.MockLogException<UnauthorizedException>();

            //Act
            Exception exception = await Record.ExceptionAsync(() => userLogic.ChangeUserPasswordAsync(user.Id, password, null));

            //Assert
            Assert.IsType<UnauthorizedException>(exception);

            RunVerification();
        }

        [Fact]
        public async Task ChangeUserPasswordAsync_NullUser_ThrowsUnAuthorizedException()
        {
            //arrange
            mockUserDAO.MockGetAsync(1, null);
            mockLogManager.MockLogMessage("User not found");
            mockLogManager.MockLogException<UnauthorizedException>();

            //Act
            Exception exception = await Record.ExceptionAsync(() => userLogic.ChangeUserPasswordAsync(1, null, null));

            //Assert
            Assert.IsType<UnauthorizedException>(exception);

            RunVerification();
        }

        private void SetUpUserNameAndEmailUniqueChecks(User emailUser, User userNameUser)
        {
            mockUserDAO.MockGetUserByEmailAsync(user.EmailAddress, emailUser);
            mockUserDAO.MockGetUserByUserNameAsync(user.UserName, userNameUser);
        }

        private void SetUpLoginUserAsync(User user)
        {
            mockUserDAO.MockGetUserByUserNameAsync(userName, user);
            mockPasswordHashProvider.MockComputeHash(password, hashedPassword);
        }

        private void SetUpChangeUserPasswordAsync(User user)
        {
            mockUserDAO.MockGetAsync(user.Id, user);
            mockPasswordHashProvider.MockComputeHash(password, hashedPassword);
        }

        private void RunVerification()
        {
            mockUserDAO.RunVerification();
            mockLogManager.RunVerification();
            mockPasswordHashProvider.RunVerification();
            mockLoginHandler.RunVerification();
        }
    }
}
