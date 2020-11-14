using CalorieMonitor.Core.Entities;
using CalorieMonitor.Core.Implementations;
using CalorieMonitor.Core.Interfaces;
using System.Threading.Tasks;

namespace CalorieMonitor.Data.Interfaces
{
    public interface IUserDAO : ICoreDAO<User>
    {
        Task<SearchResult<User>> SearchUsersAsync(IFilter filter, int startIndex, int limit);
        Task<User> GetUserByEmailAsync(string email);
        Task<User> GetUserByUserNameAsync(string username);
    }
}
