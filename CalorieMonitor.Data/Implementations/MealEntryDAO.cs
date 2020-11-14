using CalorieMonitor.Core.Entities;
using CalorieMonitor.Core.Implementations;
using CalorieMonitor.Core.Interfaces;
using CalorieMonitor.Data.Interfaces;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace CalorieMonitor.Data.Implementations
{
    public class MealEntryDAO : CoreDAO<MealEntry>, IMealEntryDAO
    {
        private readonly IDbFilterQueryHandler filterQueryHandler;
        private const string INSERT_QUERY = "Insert into MealEntries (EntryUserId, EntryDateTime, Calories, Text, EntryCreatorId, CaloriesStatus, WithInDailyLimit, DateCreated, DateUpdated) values(@EntryUserId, @EntryDateTime, @Calories, @Text, @EntryCreatorId, @CaloriesStatus, @WithInDailyLimit, @DateCreated, @DateUpdated); SELECT CAST(SCOPE_IDENTITY() as bigint)";
        private const string UPDATE_QUERY = "Update MealEntries set EntryUserId = @EntryUserId, EntryDateTime = @EntryDateTime, Calories = @Calories, Text = @Text, EntryCreatorId = @EntryCreatorId, CaloriesStatus = @CaloriesStatus, WithInDailyLimit = @WithInDailyLimit, DateCreated = @DateCreated, DateUpdated = @DateUpdated where Id = @Id;";
        private const string SELECT_QUERY = "Select * from MealEntries MealEntry inner join Users EntryUser on MealEntry.EntryUserId = EntryUser.Id inner join Users EntryCreator on MealEntry.EntryCreatorId = EntryCreator.Id";
        private const string DELETE_QUERY = "Delete from MealItems where MealentryId = @Id; Delete from MealEntries where Id = @Id;";
        private const string COUNT_QUERY = "Select count(MealEntry.Id) from MealEntries MealEntry inner join Users EntryUser on MealEntry.EntryUserId = EntryUser.Id inner join Users EntryCreator on MealEntry.EntryCreatorId = EntryCreator.Id";
        protected static string PAGING_QUERY = "Order by MealEntry.Id OFFSET @Start ROWS FETCH NEXT @Limit ROWS ONLY";
        protected static string TOTAL_CALORIES_IN_A_DAY_QUERY = "Select sum(Calories) from MealEntries where EntryUserId = @UserId and Convert(date,EntryDateTime) = @Date";


        private readonly static CRUDQueryObject mealEntryCRUDQueryObject = new CRUDQueryObject
        {
            DeleteQuery = DELETE_QUERY,
            InsertQuery = INSERT_QUERY,
            SelectQuery = $"{SELECT_QUERY} where Id = @Id",
            UpdateQuery = UPDATE_QUERY
        };

        public MealEntryDAO(IDbConnectionProvider connectionProvider, IDbFilterQueryHandler filterQueryHandler)
            : base(connectionProvider, mealEntryCRUDQueryObject)
        {
            this.filterQueryHandler = filterQueryHandler ?? throw new ArgumentNullException("filterQueryHandler");
        }
        public async Task<MealEntry> GetEntryAsync(long id)
        {
            IDbConnection connection = await connectionProvider.GetDbConnectionAsync();
            var entries = await connection.QueryAsync<MealEntry, User, User, MealEntry>(
                $"{SELECT_QUERY} where MealEntry.Id = @Id",
                MapFromDapperToMealEntry,
                new User { Id = id });
            return entries.FirstOrDefault();
        }

        public async Task<SearchResult<MealEntry>> SearchEntriesAsync(IFilter filter, int startIndex, int limit)
        {
            IDbConnection connection = await connectionProvider.GetDbConnectionAsync();
            string searchEntriesQuery = filterQueryHandler.GenerateQuery(filter, out List<QueryParam> queryParams);

            DynamicParameters searchEntriesparameters = GenerateSearchParameters(queryParams, startIndex, limit);
            DynamicParameters searchEntriesCountParameters = GenerateParameters(queryParams);

            List<MealEntry> items = (await connection.QueryAsync<MealEntry, User, User, MealEntry>(
                $"{SELECT_QUERY} {searchEntriesQuery} {PAGING_QUERY}",
                MapFromDapperToMealEntry,
                searchEntriesparameters))?.AsList();
            long totalCount = await connection.ExecuteScalarAsync<long>($"{COUNT_QUERY} {searchEntriesQuery}", searchEntriesCountParameters);
            return new SearchResult<MealEntry> { Results = items, TotalCount = totalCount };
        }

        public async Task<double> GetTotalCaloriesForUserInCurrentDateAsync(long userId, DateTime date)
        {
            IDbConnection connection = await connectionProvider.GetDbConnectionAsync();
            double totalCalories = await connection.ExecuteScalarAsync<double>(TOTAL_CALORIES_IN_A_DAY_QUERY, new { UserId = userId, Date = date });
            return totalCalories;
        }


        private MealEntry MapFromDapperToMealEntry(MealEntry entry, User entryOwner, User entryCreator)
        {
            entry.EntryUser = entryOwner;
            entry.EntryCreator = entryCreator;
            return entry;
        }
    }
}
