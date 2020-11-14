using CalorieMonitor.Core.Entities;
using System.Threading.Tasks;

namespace CalorieMonitor.Data.Interfaces
{
    public interface ICoreDAO<T> where T : Entity
    {
        Task<T> InsertAsync(T entity);
        Task<T> UpdateAsync(T entity);
        Task<T> GetAsync(long id);
        Task<bool> DeleteAsync(long id);
    }
}
