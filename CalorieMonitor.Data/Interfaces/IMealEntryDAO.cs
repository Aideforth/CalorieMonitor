using CalorieMonitor.Core.Entities;
using CalorieMonitor.Core.Implementations;
using CalorieMonitor.Core.Interfaces;
using System;
using System.Threading.Tasks;

namespace CalorieMonitor.Data.Interfaces
{
    public interface IMealEntryDAO : ICoreDAO<MealEntry>
    {
        Task<SearchResult<MealEntry>> SearchEntriesAsync(IFilter filter, int startIndex, int limit);
        Task<MealEntry> GetEntryAsync(long id);
        Task<double> GetTotalCaloriesForUserInCurrentDateAsync(long userId, DateTime date);
    }
}
