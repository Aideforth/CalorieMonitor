using CalorieMonitor.Core.Entities;
using CalorieMonitor.Core.Enums;
using CalorieMonitor.Core.Exceptions;
using CalorieMonitor.Core.Implementations;
using CalorieMonitor.Core.Interfaces;
using CalorieMonitor.Core.Models;
using CalorieMonitor.Logic.Interfaces;
using CalorieMonitor.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CalorieMonitor.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsersController : BaseController
    {
        private readonly IUserLogic userLogic;
        private readonly ISearchFilterHandler searchFilterHandler;

        public UsersController(IUserLogic userLogic, ISearchFilterHandler searchFilterHandler)
        {
            this.userLogic = userLogic ?? throw new ArgumentNullException("userLogic");
            this.searchFilterHandler = searchFilterHandler ?? throw new ArgumentNullException("searchFilterHandler");
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> CreateUser([FromBody]CreateUserDTO createUserDTO)
        {
            try
            {
                if (createUserDTO == null || !ModelState.IsValid)
                {
                    if (createUserDTO == null) ModelState.AddModelError("Result", "No Payload was sent");
                    return BadRequest(ModelState);
                }
                User user = new User
                {
                    EmailAddress = createUserDTO.EmailAddress,
                    FirstName = createUserDTO.FirstName,
                    LastName = createUserDTO.LastName,
                    Password = createUserDTO.Password,
                    UserName = createUserDTO.UserName,
                    Role = createUserDTO.Role,
                    DailyCalorieLimit = createUserDTO.DailyCalorieLimit
                };

                if (user.Role != UserRole.Regular && !ValidateAccess(user, true))
                {
                    throw new UnauthorizedException($"Only Admins are allowed to create user with role {user.Role}");
                }

                user = await userLogic.CreateAsync(user);

                return Created($"api/users/{user.Id}", new ApiObjectResponse<User>
                {
                    IsSuccessful = true,
                    Message = "Successful",
                    Data = user
                });
            }
            catch (Exception ex)
            {
                return PrepareException(ex);
            }
        }

        [HttpGet]
        [Route("{id}")]
        [Authorize(Policy = Policies.ManageUsers)]
        public async Task<IActionResult> GetUser(long id)
        {
            try
            {
                var searchResult = await userLogic.GetAsync(id);

                ValidateRecord(searchResult);

                return Ok(new ApiObjectResponse<User>
                {
                    IsSuccessful = true,
                    Message = "Successful",
                    Data = searchResult
                });
            }
            catch (Exception ex)
            {
                return PrepareException(ex);
            }
        }

        [HttpGet]
        [Route("own")]
        public async Task<IActionResult> GetSelf()
        {
            long userId = long.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            return await GetUser(userId);
        }

        [HttpPatch]
        [Route("{id}")]
        [Authorize(Policy = Policies.ManageUsers)]
        public async Task<IActionResult> PatchUser([FromBody]PatchUserDTO patchUserDTO, long id)
        {
            try
            {
                var user = await userLogic.GetAsync(id);

                ValidateRecord(user);

                if (patchUserDTO == null || !ModelState.IsValid)
                {
                    if (patchUserDTO == null) ModelState.AddModelError("Result", "No Payload was sent");
                    return BadRequest(ModelState);
                }

                if (patchUserDTO.Role.HasValue 
                    && patchUserDTO.Role.Value != user.Role
                    && !ValidateAccess(user, true))
                {
                    throw new BusinessException("Only Admins are allowed to change user role");
                }

                //apply patch values
                if (patchUserDTO.EmailAddress != null) user.EmailAddress = patchUserDTO.EmailAddress;
                if (patchUserDTO.FirstName != null) user.FirstName = patchUserDTO.FirstName;
                if (patchUserDTO.LastName != null) user.LastName = patchUserDTO.LastName;
                if (patchUserDTO.UserName != null) user.UserName = patchUserDTO.UserName;
                if (patchUserDTO.Role.HasValue) user.Role = patchUserDTO.Role.Value;
                if (patchUserDTO.DailyCalorieLimit.HasValue) user.DailyCalorieLimit = patchUserDTO.DailyCalorieLimit.Value;
                user = await userLogic.UpdateAsync(user);

                return Ok(new ApiObjectResponse<User>
                {
                    IsSuccessful = true,
                    Message = "Successful",
                    Data = user
                });
            }
            catch (Exception ex)
            {
                return PrepareException(ex);
            }
        }

        [HttpPatch]
        [Route("own")]
        public async Task<IActionResult> PatchSelf([FromBody]PatchUserDTO patchUserDTO)
        {
            long userId = long.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            return await PatchUser(patchUserDTO, userId);
        }

        [HttpDelete]
        [Route("{id}")]
        [Authorize(Policy = Policies.ManageUsers)]
        public async Task<IActionResult> DeleteUser(long id)
        {
            try
            {
                var record = await userLogic.GetAsync(id);

                ValidateRecord(record);

                var deleteResult = await userLogic.DeleteAsync(id);
                return Ok(new ApiResponse
                {
                    IsSuccessful = deleteResult,
                    Message = "Successful"
                });
            }
            catch (Exception ex)
            {
                return PrepareException(ex);
            }
        }

        [HttpGet]
        [Authorize(Policy = Policies.ManageUsers)]
        public async Task<IActionResult> SearchUsers(string filter, int start = 0, int limit = 10)
        {
            try
            {
                IFilter filterObj = searchFilterHandler.Parse(filter, typeof(User));
                SearchResult<User> searchResult = await userLogic.SearchUsersAsync(filterObj, start, limit);
                return Ok(new ApiObjectResponse<SearchResult<User>>
                {
                    IsSuccessful = true,
                    Message = "Successful",
                    Data = searchResult
                });
            }
            catch (Exception ex)
            {
                return PrepareException(ex);
            }
        }

        [HttpPost]
        [Route("login")]
        [AllowAnonymous]
        public async Task<IActionResult> LoginUser([FromBody]LoginRequest loginRequest)
        {
            try
            {
                if (loginRequest == null || !ModelState.IsValid)
                {
                    if (loginRequest == null) ModelState.AddModelError("Result", "No Payload was sent");
                    return BadRequest(ModelState);
                }

                string token = await userLogic.LoginUserAsync(loginRequest.UserName, loginRequest.Password);

                return Ok(new ApiObjectResponse<TokenInfo>
                {
                    IsSuccessful = true,
                    Message = "Successful",
                    Data = new TokenInfo
                    {
                        Token = token
                    }
                });
            }
            catch (Exception ex)
            {
                return PrepareException(ex);
            }
        }

        [HttpPost]
        [Route("own/change-password")]
        public async Task<IActionResult> ChangePassword([FromBody]ChangePasswordRequest changePasswordRequest)
        {
            try
            {
                if (changePasswordRequest == null || !ModelState.IsValid)
                {
                    if (changePasswordRequest == null) ModelState.AddModelError("Result", "No Payload was sent");
                    return BadRequest(ModelState);
                }

                long userId = long.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                await userLogic.ChangeUserPasswordAsync(userId,
                    changePasswordRequest.OldPassword,
                    changePasswordRequest.NewPassword);

                return Ok(new ApiResponse
                {
                    IsSuccessful = true,
                    Message = "Successful"
                });
            }
            catch (Exception ex)
            {
                return PrepareException(ex);
            }
        }

        private void ValidateRecord(User user, bool onlyAdmins = false)
        {
            if (user == null)
            {
                throw new NotFoundException("User Not Found");
            }

            if (!ValidateAccess(user, onlyAdmins))
            {
                throw new UnauthorizedException();
            }
        }

        private bool ValidateAccess(User user, bool onlyAdmins)
        {

            string userIdString = HttpContext.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            string userRoleString = HttpContext.User?.FindFirst(ClaimTypes.Role)?.Value;

            if(string.IsNullOrEmpty(userIdString) || string.IsNullOrEmpty(userRoleString))
            {
                return false;
            }

            if (Enum.TryParse(userRoleString, out UserRole role))
            {
                if (onlyAdmins) return role == UserRole.Admin;
                //Admins change all, Usermanager can change all Regulars
                if (role == UserRole.Admin || (user.Role == UserRole.Regular && role == UserRole.UserManager))
                {
                    return true;
                }
            }
            else if (onlyAdmins)
            {
                return false;
            }
            if (long.TryParse(userIdString, out long userId) && userId == user.Id)
            {
                return true;
            }
            return false;
        }
    }
}
