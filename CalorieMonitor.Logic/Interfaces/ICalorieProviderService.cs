using CalorieMonitor.Logic.Implementations;
using System.Threading.Tasks;

namespace CalorieMonitor.Logic.Interfaces
{
    public interface ICalorieProviderService
    {
        Task<CalorieServiceResult> GetCalorieResultAsync(string text);
    }
}
