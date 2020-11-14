using CalorieMonitor.Core.Entities;
using CalorieMonitor.Core.Enums;
using CalorieMonitor.Core.Exceptions;
using CalorieMonitor.Data.Interfaces;
using CalorieMonitor.Logic.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace CalorieMonitor.Logic.Implementations
{
    public class MealItemLogic : IMealItemLogic
    {
        private readonly IMealItemDAO mealItemDAO;
        private readonly IMealEntryDAO mealEntryDAO;
        private readonly ICalorieProviderService calorieProviderService;
        private readonly ILogManager logManager;

        public MealItemLogic(IMealItemDAO mealItemDAO, IMealEntryDAO mealEntryDAO, ICalorieProviderService calorieProviderService, ILogManager logManager)
        {
            this.mealItemDAO = mealItemDAO ?? throw new ArgumentNullException("mealItemDAO");
            this.mealEntryDAO = mealEntryDAO ?? throw new ArgumentNullException("mealEntryDAO");
            this.calorieProviderService = calorieProviderService ?? throw new ArgumentNullException("calorieProviderService");
            this.logManager = logManager ?? throw new ArgumentNullException("logManager");
        }

        public async Task ProcessMealItemsAsync(MealEntry entry)
        {
            try
            {
                double totalCalories = 0;
                CalorieServiceResult result = await calorieProviderService.GetCalorieResultAsync(entry.Text);
                List<CalorieServiceFood> foods = result?.Foods ?? new List<CalorieServiceFood>();
                List<MealItem> mealItems = await mealItemDAO.GetItemsByMealEntryIdAsync(entry.Id) ?? new List<MealItem>();

                IEnumerable<string> foodNames = foods.Select(c => c.FoodName);
                IEnumerable<string> mealItemNames = mealItems.Select(c => c.Name);
                List<MealItem> itemsToUpdate = mealItems.Where(v => foodNames.Contains(v.Name)).ToList();
                List<MealItem> itemsToDelete = mealItems.Where(v => !foodNames.Contains(v.Name)).ToList();
                List<CalorieServiceFood> itemsToCreate = foods.Where(v => !mealItemNames.Contains(v.FoodName)).ToList();

                logManager.LogMessage($"For Meal Entry Id {entry.Id}: {itemsToUpdate.Count} to update, {itemsToDelete.Count} to delete, {itemsToCreate.Count} to create");

                if (itemsToUpdate.Count > 0)
                {
                    foreach (MealItem item in itemsToUpdate)
                    {
                        CalorieServiceFood food = foods.First(v => v.FoodName == item.Name);
                        SetCalorieInfo(item, food);
                        totalCalories += food.Calories;
                        item.DateUpdated = DateTime.UtcNow;
                        await mealItemDAO.UpdateAsync(item);
                    }
                }
                if (itemsToDelete.Count > 0)
                {
                    foreach (MealItem item in itemsToDelete)
                    {
                        await mealItemDAO.DeleteAsync(item.Id);
                    }
                }
                if (itemsToCreate.Count > 0)
                {
                    foreach (CalorieServiceFood food in itemsToCreate)
                    {
                        MealItem newItem = new MealItem
                        {
                            MealEntryId = entry.Id,
                            Name = food.FoodName,
                            DateCreated = DateTime.UtcNow
                        };
                        SetCalorieInfo(newItem, food);
                        totalCalories += food.Calories;
                        await mealItemDAO.InsertAsync(newItem);
                    }
                }

                //only calculate for new entries or those whose calorie count has not yet impacted the daily limit
                if (entry.Calories == 0)
                {
                    double totalCaloriesForDate = await mealEntryDAO.
                            GetTotalCaloriesForUserInCurrentDateAsync(entry.EntryUser.Id, entry.EntryDateTime.Date);
                    entry.WithInDailyLimit = (totalCaloriesForDate + totalCalories) < entry.EntryUser.DailyCalorieLimit;
                }

                entry.Calories = totalCalories;
                entry.CaloriesStatus = foods.Count > 0 ? CaloriesStatus.AppProcessed : CaloriesStatus.NoInfoFound;
                entry.EntryUserId = entry.EntryUser.Id;
                entry.EntryCreatorId = entry.EntryCreator.Id;
                await mealEntryDAO.UpdateAsync(entry);
            }
            catch (Exception ex)
            {
                logManager.LogException(ex);
                throw new FailureException("An error occurred while processing meal items, please try again");
            }
        }

        private static void SetCalorieInfo(MealItem item, CalorieServiceFood food)
        {
            item.Calories = food.Calories;
            item.WeightInGrams = food.ServingWeightGrams;
            item.CaloriePerGram = food.Calories / food.ServingWeightGrams;
        }
    }
}
