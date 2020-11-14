using CalorieMonitor.Core.Entities;
using CalorieMonitor.Core.Implementations;
using CalorieMonitor.Core.Interfaces;
using System.Threading.Tasks;

namespace CalorieMonitor.Logic.Interfaces
{
    public interface IMealEntryLogic
    {
        Task<MealEntry> SaveAsync(MealEntry entry);
        Task<MealEntry> UpdateAsync(MealEntry entry);
        Task<MealEntry> GetAsync(long id);
        Task<bool> DeleteAsync(long id);
        Task<SearchResult<MealEntry>> SearchEntriesAsync(IFilter filter, int startIndex, int limit);
    }
}
