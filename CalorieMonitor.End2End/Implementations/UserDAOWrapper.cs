using CalorieMonitor.Data.Implementations;
using CalorieMonitor.Data.Interfaces;

namespace CalorieMonitor.End2End.Implementations
{
    public class UserDAOWrapper : UserDAO, IUserDAO
    {
        public UserDAOWrapper(IDbConnectionProvider connectionProvider, IDbFilterQueryHandler filterQueryHandler)
            : base(connectionProvider, filterQueryHandler)
        {
            queryObject.InsertQuery = "Insert into Users (FirstName, LastName, Role, UserName, Password, EmailAddress, DailyCalorieLimit, DateCreated, DateUpdated) values(@FirstName, @LastName, @Role, @UserName, @Password, @EmailAddress, @DailyCalorieLimit, @DateCreated, @DateUpdated); SELECT CAST(last_insert_rowid() as bigint)";
            PAGING_QUERY = "ORDER BY Id LIMIT @Limit OFFSET @Start;";
        }
    }
}
