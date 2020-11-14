using CalorieMonitor.Core.Entities;
using CalorieMonitor.Data.Interfaces;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace CalorieMonitor.Data.Implementations
{
    public class MealItemDAO : CoreDAO<MealItem>, IMealItemDAO
    {
        private const string INSERT_QUERY = "Insert into MealItems (Name, Calories, WeightInGrams, CaloriePerGram, MealEntryId, DateCreated, DateUpdated) values(@Name, @Calories, @WeightInGrams, @CaloriePerGram, @MealEntryId, @DateCreated, @DateUpdated); SELECT CAST(SCOPE_IDENTITY() as bigint)";
        private const string UPDATE_QUERY = "Update MealItems set Name = @Name, Calories = @Calories, WeightInGrams = @WeightInGrams, CaloriePerGram = @CaloriePerGram, MealEntryId = @MealEntryId, DateCreated = @DateCreated, DateUpdated = @DateUpdated where Id = @Id;";
        private const string SELECT_QUERY = "Select * from MealItems";
        private const string DELETE_QUERY = "Delete from MealItems where Id = @Id;";

        private readonly static CRUDQueryObject mealItemCRUDQueryObject = new CRUDQueryObject
        {
            DeleteQuery = DELETE_QUERY,
            InsertQuery = INSERT_QUERY,
            SelectQuery = $"{SELECT_QUERY} where Id = @Id",
            UpdateQuery = UPDATE_QUERY
        };

        public MealItemDAO(IDbConnectionProvider connectionProvider)
            : base(connectionProvider, mealItemCRUDQueryObject)
        {
        }

        public async Task<List<MealItem>> GetItemsByMealEntryIdAsync(long id)
        {
            IDbConnection connection = await connectionProvider.GetDbConnectionAsync();
            var result = await connection.QueryAsync<MealItem>($"{SELECT_QUERY} where MealEntryId = @MealEntryId", new MealItem { MealEntryId = id });
            return result?.AsList() ?? new List<MealItem>();
        }
    }
}
