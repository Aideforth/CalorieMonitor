using CalorieMonitor.Core.Entities;
using CalorieMonitor.Core.Enums;
using CalorieMonitor.Core.Exceptions;
using CalorieMonitor.Core.Implementations;
using CalorieMonitor.Core.Interfaces;
using CalorieMonitor.Data.Interfaces;
using CalorieMonitor.Logic.Interfaces;
using Hangfire;
using System;
using System.Threading.Tasks;

namespace CalorieMonitor.Logic.Implementations
{
    public class MealEntryLogic : IMealEntryLogic
    {
        private readonly IMealEntryDAO mealEntryDAO;
        private readonly IMealItemLogic mealItemLogic;
        private readonly IBackgroundJobClient _jobClient;
        private readonly ILogManager logManager;
        public MealEntryLogic(IMealEntryDAO mealEntryDAO, IMealItemLogic mealItemLogic, ILogManager logManager, IBackgroundJobClient _jobClient)
        {
            this.mealEntryDAO = mealEntryDAO ?? throw new ArgumentNullException("mealEntryDAO");
            this.mealItemLogic = mealItemLogic ?? throw new ArgumentNullException("mealItemLogic");
            this.logManager = logManager ?? throw new ArgumentNullException("logManager");
            this._jobClient = _jobClient ?? throw new ArgumentNullException("_jobClient");
        }
        public async Task<bool> DeleteAsync(long id)
        {
            try
            {
                await mealEntryDAO.DeleteAsync(id);
                return true;
            }
            catch (Exception ex)
            {
                logManager.LogException(ex);
                throw new FailureException("An error occurred while deleting the entry, please try again");
            }
        }

        public async Task<MealEntry> GetAsync(long id)
        {
            try
            {
                return await mealEntryDAO.GetEntryAsync(id);
            }
            catch (Exception ex)
            {
                logManager.LogException(ex);
                throw new FailureException("An error occurred while retrieving the entry, please try again");
            }
        }

        public async Task<MealEntry> SaveAsync(MealEntry entry)
        {
            try
            {
                entry.DateCreated = DateTime.UtcNow;
                ValidateThatTimeIsNotInTheFuture(entry);

                if (entry.CaloriesStatus == CaloriesStatus.CustomerProvided)
                {
                    double totalCaloriesForDate = await mealEntryDAO.
                        GetTotalCaloriesForUserInCurrentDateAsync(entry.EntryUserId, entry.EntryDateTime.Date);
                    entry.WithInDailyLimit = (totalCaloriesForDate + entry.Calories) < entry.EntryUser.DailyCalorieLimit;
                }
                entry = await mealEntryDAO.InsertAsync(entry);
                
                //process calorie count with hangfire
                ProcessCalorieCount(entry);
                return entry;
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, "An error occurred while saving the entry, please try again");
            }
        }

        public async Task<SearchResult<MealEntry>> SearchEntriesAsync(IFilter filter, int startIndex, int limit)
        {
            try
            {
                return await mealEntryDAO.SearchEntriesAsync(filter, startIndex, limit);
            }
            catch (Exception ex)
            {
                logManager.LogException(ex);
                throw new FailureException("An error occurred while retrieving the entries, please try again");
            }
        }

        public async Task<MealEntry> UpdateAsync(MealEntry entry)
        {
            try
            {
                entry.DateUpdated = DateTime.UtcNow;
                ValidateThatTimeIsNotInTheFuture(entry);

                entry =  await mealEntryDAO.UpdateAsync(entry);

                //process calorie count with hangfire
                ProcessCalorieCount(entry);
                return entry;
            }
            catch (Exception ex)
            {
                throw ProcessException(ex, "An error occurred while updating the entry, please try again");
            }
        }

        private void ProcessCalorieCount(MealEntry entry)
        {
            if (entry.CaloriesStatus == CaloriesStatus.Pending)
            {
                _jobClient.Enqueue(() => mealItemLogic.ProcessMealItemsAsync(entry));
            }
        }

        private void ValidateThatTimeIsNotInTheFuture(MealEntry entry)
        {
            if (entry.EntryDateTime > DateTime.UtcNow)
            {
                throw new BusinessException("Entry DateTime is in the future");
            }
        }

        private Exception ProcessException(Exception ex, string failureMessage)
        {
            logManager.LogException(ex);

            if (ex is BusinessException)
            {
                return ex;
            }
            return new FailureException(failureMessage);
        }
    }
}
