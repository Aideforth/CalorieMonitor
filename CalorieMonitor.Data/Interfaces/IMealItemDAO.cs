using CalorieMonitor.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CalorieMonitor.Data.Interfaces
{
    public interface IMealItemDAO : ICoreDAO<MealItem>
    {
        Task<List<MealItem>> GetItemsByMealEntryIdAsync(long id);
    }
}
