using CalorieMonitor.Core.Entities;
using CalorieMonitor.Core.Enums;
using CalorieMonitor.Core.Exceptions;
using CalorieMonitor.Core.Implementations;
using CalorieMonitor.Core.Interfaces;
using CalorieMonitor.Core.Models;
using CalorieMonitor.Logic.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CalorieMonitor.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MealsController : BaseController
    {
        private readonly IMealEntryLogic mealEntryLogic;
        private readonly IUserLogic userLogic;
        private readonly ISearchFilterHandler searchFilterHandler;

        public MealsController(IMealEntryLogic mealEntryLogic, IUserLogic userLogic, ISearchFilterHandler searchFilterHandler)
        {
            this.userLogic = userLogic ?? throw new ArgumentNullException("userLogic");
            this.mealEntryLogic = mealEntryLogic ?? throw new ArgumentNullException("mealEntryLogic");
            this.searchFilterHandler = searchFilterHandler ?? throw new ArgumentNullException("searchFilterHandler");
        }

        [HttpPost]
        public async Task<IActionResult> AddMealEntry([FromBody]CreateMealEntryDTO createMealEntryDTO)
        {
            try
            {
                if (createMealEntryDTO == null || !ModelState.IsValid)
                {
                    if (createMealEntryDTO == null) ModelState.AddModelError("Result", "No Payload was sent");
                    return BadRequest(ModelState);
                }
                string userIdString = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;

                MealEntry entry = new MealEntry
                {
                    Calories = createMealEntryDTO.Calories,
                    Text = createMealEntryDTO.Text,
                    EntryDateTime = createMealEntryDTO.EntryDateTime.ToUniversalTime()
                };

                if (createMealEntryDTO.EntryUser?.Id > 0)
                {
                    entry.EntryUserId = createMealEntryDTO.EntryUser.Id;
                    if (!ValidateAccess(entry))
                    {
                        throw new UnauthorizedException();
                    }
                    entry.EntryCreatorId = long.Parse(userIdString);
                }
                else
                {
                    entry.EntryUserId = long.Parse(userIdString);
                    entry.EntryCreatorId = long.Parse(userIdString);
                }

                User user = await userLogic.GetAsync(entry.EntryUserId);
                if (user == null)
                {
                    ModelState.AddModelError("EntryUserId", "The EntryUserId provided does not exist");
                    return BadRequest(ModelState);
                }
                entry.EntryUser = user;
                if (entry.EntryCreatorId == user.Id)
                {
                    entry.EntryCreator = user;
                }
                else
                {
                    User currentUser = await userLogic.GetAsync(entry.EntryCreatorId);
                    entry.EntryCreator = currentUser;
                }
                entry.CaloriesStatus = entry.Calories > 0 ? CaloriesStatus.CustomerProvided : CaloriesStatus.Pending;
                entry = await mealEntryLogic.SaveAsync(entry);

                return Created($"api/mealEntry/{entry.Id}", new ApiObjectResponse<MealEntry>
                {
                    IsSuccessful = true,
                    Message = "Successful",
                    Data = entry
                });
            }
            catch (Exception ex)
            {
                return PrepareException(ex);
            }
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetMealEntry(long id)
        {
            try
            {
                var searchResult = await mealEntryLogic.GetAsync(id);

                ValidateEntry(searchResult);

                return Ok(new ApiObjectResponse<MealEntry>
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

        [HttpPatch]
        [Route("{id}")]
        public async Task<IActionResult> PatchMealEntry([FromBody]PatchMealEntryDTO patchMealEntryDTO, long id)
        {
            try
            {
                MealEntry entry = await mealEntryLogic.GetAsync(id);

                ValidateEntry(entry);

                if (patchMealEntryDTO == null || !ModelState.IsValid)
                {
                    if (patchMealEntryDTO == null) ModelState.AddModelError("Result", "No Payload was sent");
                    return BadRequest(ModelState);
                }

                //apply patch values
                if (patchMealEntryDTO.Text != null)
                {
                    entry.Text = patchMealEntryDTO.Text;
                    entry.Calories = 0;
                }
                if (patchMealEntryDTO.Calories.HasValue)
                {
                    entry.Calories = patchMealEntryDTO.Calories.Value;
                }
                if (patchMealEntryDTO.EntryDateTime.HasValue)
                {
                    entry.EntryDateTime = patchMealEntryDTO.EntryDateTime.Value.ToUniversalTime();
                }
                entry.CaloriesStatus = entry.Calories > 0 ? CaloriesStatus.CustomerProvided : CaloriesStatus.Pending;
                entry = await mealEntryLogic.UpdateAsync(entry);

                return Ok(new ApiObjectResponse<MealEntry>
                {
                    IsSuccessful = true,
                    Message = "Successful",
                    Data = entry
                });
            }
            catch (Exception ex)
            {
                return PrepareException(ex);
            }
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> DeleteMealEntry(long id)
        {
            try
            {
                MealEntry entry = await mealEntryLogic.GetAsync(id);

                ValidateEntry(entry);

                var deleteResult = await mealEntryLogic.DeleteAsync(id);
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
        public async Task<IActionResult> SearchMealEntries(string filter, int start = 0, int limit = 10)
        {
            try
            {
                string userRoleString = HttpContext.User.FindFirst(ClaimTypes.Role).Value;
                string userIdString = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
                IFilter filterObj = searchFilterHandler.Parse(filter, typeof(MealEntry));
                if (Enum.TryParse(userRoleString, out UserRole role) && role != UserRole.Admin)
                {
                    filterObj = RestrictFilterWithUserId(userIdString, filterObj);
                }

                SearchResult<MealEntry> searchResult = await mealEntryLogic.SearchEntriesAsync(filterObj, start, limit);
                return Ok(new ApiObjectResponse<SearchResult<MealEntry>>
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

        private static IFilter RestrictFilterWithUserId(string userIdString, IFilter filterObj)
        {
            IFilter userFilterObj = new FieldFilter("MealEntry.EntryUserId", FilterComparison.Equals, long.Parse(userIdString));
            if (filterObj == null) filterObj = userFilterObj;
            else
            {
                //but all other conditions in a bracket
                filterObj.HasBrackets = true;

                //sepcify that all entries my belong to the current user
                filterObj = new SearchFilter
                {
                    HasBrackets = true,
                    LeftHandFilter = userFilterObj,
                    Operation = FilterOperation.And,
                    RightHandFilter = filterObj
                };
            }

            return filterObj;
        }

        private void ValidateEntry(MealEntry entry)
        {
            if (entry == null)
            {
                throw new NotFoundException("Meal Entry Not Found");
            }

            if (!ValidateAccess(entry))
            {
                throw new UnauthorizedException();
            }
        }
        private bool ValidateAccess(MealEntry entry)
        {

            string userIdString = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            string userRoleString = HttpContext.User.FindFirst(ClaimTypes.Role).Value;

            if (Enum.TryParse(userRoleString, out UserRole role) && role == UserRole.Admin)
            {
                return true;
            }
            if (long.TryParse(userIdString, out long userId) && userId == entry.EntryUserId)
            {
                return true;
            }
            return false;
        }
    }
}
