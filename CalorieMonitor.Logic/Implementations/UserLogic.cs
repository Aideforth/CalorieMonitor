using CalorieMonitor.Core.Entities;
using CalorieMonitor.Core.Exceptions;
using CalorieMonitor.Core.Implementations;
using CalorieMonitor.Core.Interfaces;
using CalorieMonitor.Data.Interfaces;
using CalorieMonitor.Logic.Interfaces;
using System;
using System.Threading.Tasks;

namespace CalorieMonitor.Logic.Implementations
{
    public class UserLogic : IUserLogic
    {
        readonly IUserDAO userDAO;
        readonly IPasswordHashProvider hashProvider;
        readonly ILoginHandler loginHandler;
        readonly ILogManager logManager;
        public UserLogic(IUserDAO userDAO, IPasswordHashProvider hashProvider, ILoginHandler loginHandler, ILogManager logManager)
        {
            this.userDAO = userDAO ?? throw new ArgumentNullException("userDAO");
            this.hashProvider = hashProvider ?? throw new ArgumentNullException("hashProvider");
            this.loginHandler = loginHandler ?? throw new ArgumentNullException("loginHandler");
            this.logManager = logManager ?? throw new ArgumentNullException("logManager");
        }

        public async Task<User> CreateAsync(User user)
        {
            try
            {
                User emailUser = await userDAO.GetUserByEmailAsync(user.EmailAddress);
                if (emailUser != null)
                {
                    throw new BusinessException("The email provided is already registered");
                }

                User usernameUser = await userDAO.GetUserByUserNameAsync(user.UserName);
                if (usernameUser != null)
                {
                    throw new BusinessException("The username provided is already registered");
                }

                user.Password = hashProvider.ComputeHash(user.Password);
                user.DateCreated = DateTime.UtcNow;
                //if(user.DateUpdated < (DateTime)SqlDateTime.MinValue) user.DateUpdated = (DateTime)SqlDateTime.MinValue;
                return await userDAO.InsertAsync(user);
            }
            catch (BusinessException ex)
            {
                logManager.LogException(ex);
                throw ex;
            }
            catch (Exception ex)
            {
                logManager.LogException(ex);
                throw new FailureException("An error occurred while saving the user, please try again");
            }
        }

        public async Task<bool> DeleteAsync(long id)
        {
            try
            {
                User record = await userDAO.GetAsync(id);

                if (record == null)
                {
                    throw new NotFoundException("The user does not exist");
                }

                await userDAO.DeleteAsync(id);
                return true;
            }
            catch (NotFoundException ex)
            {
                logManager.LogException(ex);
                throw ex;
            }
            catch (Exception ex)
            {
                logManager.LogException(ex);
                throw new FailureException("An error occurred while deleting the user, please try again");
            }
        }

        public async Task<User> GetAsync(long id)
        {
            try
            {
                return await userDAO.GetAsync(id);
            }
            catch (Exception ex)
            {
                logManager.LogException(ex);
                throw new FailureException("An error occurred while retrieving the user, please try again");
            }
        }

        public async Task<SearchResult<User>> SearchUsersAsync(IFilter filter, int startIndex, int limit)
        {
            try
            {
                return await userDAO.SearchUsersAsync(filter, startIndex, limit);
            }
            catch (Exception ex)
            {
                logManager.LogException(ex);
                throw new FailureException("An error occurred while retrieving the users, please try again");
            }
        }

        public async Task<User> UpdateAsync(User user)
        {

            try
            {
                User emailUser = await userDAO.GetUserByEmailAsync(user.EmailAddress);
                if (emailUser != null && emailUser.Id != user.Id)
                {
                    throw new BusinessException("The email provided is already registered to another user");
                }

                User usernameUser = await userDAO.GetUserByUserNameAsync(user.UserName);
                if (usernameUser != null && usernameUser.Id != user.Id)
                {
                    throw new BusinessException("The username provided is already registered to another user");
                }

                user.DateUpdated = DateTime.UtcNow;
                return await userDAO.UpdateAsync(user);
            }
            catch (BusinessException ex)
            {
                logManager.LogException(ex);
                throw ex;
            }
            catch (Exception ex)
            {
                logManager.LogException(ex);
                throw new FailureException("An error occurred while updating the user, please try again");
            }
        }

        public async Task<string> LoginUserAsync(string userName, string password)
        {
            try
            {
                var user = await userDAO.GetUserByUserNameAsync(userName);
                if (user == null)
                {
                    logManager.LogMessage("Invalid Username");
                    throw new UnauthorizedException();
                }

                string passwordHash = hashProvider.ComputeHash(password);
                if (user.Password != passwordHash)
                {
                    logManager.LogMessage("Invalid Password");
                    throw new UnauthorizedException();
                }

                return loginHandler.LoginUserAndGenerateToken(user);
            }
            catch (UnauthorizedException ex)
            {
                logManager.LogException(ex);
                throw ex;
            }
            catch (Exception ex)
            {
                logManager.LogException(ex);
                throw new FailureException("An error occurred while logging in the user, please try again");
            }
        }

        public async Task ChangeUserPasswordAsync(long id, string oldPassword, string newPassword)
        {
            try
            {
                var user = await userDAO.GetAsync(id);
                if (user == null)
                {
                    logManager.LogMessage("User not found");
                    throw new UnauthorizedException();
                }

                string oldPasswordHash = hashProvider.ComputeHash(oldPassword);
                if (user.Password != oldPasswordHash)
                {
                    logManager.LogMessage("Invalid Password");
                    throw new UnauthorizedException();
                }

                string newPasswordHash = hashProvider.ComputeHash(newPassword);
                user.Password = newPasswordHash;
                await userDAO.UpdateAsync(user);

            }
            catch (UnauthorizedException ex)
            {
                logManager.LogException(ex);
                throw ex;
            }
            catch (Exception ex)
            {
                logManager.LogException(ex);
                throw new FailureException("An error occurred while changing the user's password, please try again");
            }
        }
    }
}
