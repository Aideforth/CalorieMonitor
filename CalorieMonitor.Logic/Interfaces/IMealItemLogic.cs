using CalorieMonitor.Core.Entities;
using System.Threading.Tasks;

namespace CalorieMonitor.Logic.Interfaces
{
    public interface IMealItemLogic
    {
        Task ProcessMealItemsAsync(MealEntry entry);
    }
}
