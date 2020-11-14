using System.Data;
using System.Threading.Tasks;

namespace CalorieMonitor.Data.Interfaces
{
    public interface IDbConnectionProvider
    {
        Task<IDbConnection> GetDbConnectionAsync();
    }
}
