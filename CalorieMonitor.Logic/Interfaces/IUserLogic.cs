using CalorieMonitor.Core.Entities;
using CalorieMonitor.Core.Implementations;
using CalorieMonitor.Core.Interfaces;
using System.Threading.Tasks;

namespace CalorieMonitor.Logic.Interfaces
{
    public interface IUserLogic
    {
        Task<User> CreateAsync(User user);
        Task<User> UpdateAsync(User user);
        Task<User> GetAsync(long id);
        Task<bool> DeleteAsync(long id);
        Task<SearchResult<User>> SearchUsersAsync(IFilter filter, int startIndex, int limit);
        Task<string> LoginUserAsync(string userName, string password);
        Task ChangeUserPasswordAsync(long id, string oldPassword, string newPassword);
    }
}
