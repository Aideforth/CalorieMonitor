using CalorieMonitor.Data.Implementations;
using CalorieMonitor.Data.Interfaces;

namespace CalorieMonitor.End2End.Implementations
{
    public class MealEntryDAOWrapper : MealEntryDAO, IMealEntryDAO
    {
        public MealEntryDAOWrapper(IDbConnectionProvider connectionProvider, IDbFilterQueryHandler filterQueryHandler)
            : base(connectionProvider, filterQueryHandler)
        {
            queryObject.InsertQuery = "Insert into MealEntries (EntryUserId, EntryDateTime, Calories, Text, EntryCreatorId, CaloriesStatus, WithInDailyLimit, DateCreated, DateUpdated) values(@EntryUserId, @EntryDateTime, @Calories, @Text, @EntryCreatorId, @CaloriesStatus, @WithInDailyLimit, @DateCreated, @DateUpdated); SELECT CAST(last_insert_rowid() as bigint)";
            PAGING_QUERY = "ORDER BY Id LIMIT @Limit OFFSET @Start;";
            TOTAL_CALORIES_IN_A_DAY_QUERY = "Select sum(Calories) from MealEntries where EntryUserId = @UserId and Date(EntryDateTime) = @Date";
        }
    }
}
