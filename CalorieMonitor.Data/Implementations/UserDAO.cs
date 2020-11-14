using CalorieMonitor.Core.Entities;
using CalorieMonitor.Core.Implementations;
using CalorieMonitor.Core.Interfaces;
using CalorieMonitor.Data.Interfaces;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace CalorieMonitor.Data.Implementations
{
    public class UserDAO : CoreDAO<User>, IUserDAO
    {
        private readonly IDbFilterQueryHandler filterQueryHandler;
        private const string INSERT_QUERY = "Insert into Users (FirstName, LastName, Role, UserName, Password, EmailAddress, DailyCalorieLimit, DateCreated, DateUpdated) values(@FirstName, @LastName, @Role, @UserName, @Password, @EmailAddress, @DailyCalorieLimit, @DateCreated, @DateUpdated); SELECT CAST(SCOPE_IDENTITY() as bigint)";
        private const string UPDATE_QUERY = "Update Users set FirstName = @FirstName, LastName = @LastName, Role = @Role, UserName = @UserName, Password = @Password, EmailAddress = @EmailAddress, DailyCalorieLimit = @DailyCalorieLimit, DateCreated = @DateCreated, DateUpdated = @DateUpdated where Id = @Id;";
        private const string SELECT_QUERY = "Select * from Users";
        private const string DELETE_QUERY = "Delete from MealItems where MealEntryId in (Select Id from MealEntries where EntryUserId = @Id); Delete from MealEntries where EntryUserId = @Id; Delete from Users where Id = @Id;";
        private const string COUNT_QUERY = "Select count(Id) from Users";
        protected static string PAGING_QUERY = "ORDER BY Id OFFSET @Start ROWS FETCH NEXT @Limit ROWS ONLY";
        private readonly static CRUDQueryObject userCRUDQueryObject = new CRUDQueryObject
        {
            DeleteQuery = DELETE_QUERY,
            InsertQuery = INSERT_QUERY,
            SelectQuery = $"{SELECT_QUERY} where Id = @Id",
            UpdateQuery = UPDATE_QUERY
        };

        public UserDAO(IDbConnectionProvider connectionProvider, IDbFilterQueryHandler filterQueryHandler)
            : base(connectionProvider, userCRUDQueryObject)
        {
            this.filterQueryHandler = filterQueryHandler ?? throw new ArgumentNullException("filterQueryHandler");
        }
        public async Task<User> GetUserByEmailAsync(string email)
        {
            IDbConnection connection = await connectionProvider.GetDbConnectionAsync();
            var result = await connection.QueryFirstOrDefaultAsync<User>($"{SELECT_QUERY} where EmailAddress = @EmailAddress", new User { EmailAddress = email });
            return result;
        }

        public async Task<User> GetUserByUserNameAsync(string userName)
        {
            IDbConnection connection = await connectionProvider.GetDbConnectionAsync();
            var result = await connection.QueryFirstOrDefaultAsync<User>($"{SELECT_QUERY} where UserName = @UserName", new User { UserName = userName });
            return result;
        }

        public async Task<SearchResult<User>> SearchUsersAsync(IFilter filter, int startIndex, int limit)
        {
            IDbConnection connection = await connectionProvider.GetDbConnectionAsync();

            string searchUsersQuery = filterQueryHandler.GenerateQuery(filter,
                out List<QueryParam> queryParams);

            DynamicParameters searchUsersParameters = GenerateSearchParameters(queryParams, startIndex, limit);
            DynamicParameters searchUsersCountParameters = GenerateParameters(queryParams);

            List<User> items = (await connection.QueryAsync<User>($"{SELECT_QUERY} {searchUsersQuery} {PAGING_QUERY}", searchUsersParameters))?.AsList();
            long totalCount = await connection.ExecuteScalarAsync<long>($"{COUNT_QUERY} {searchUsersQuery}", searchUsersCountParameters);
            return new SearchResult<User> { Results = items, TotalCount = totalCount };
        }
    }
}
