using CalorieMonitor.Core.Implementations;
using CalorieMonitor.Core.Interfaces;
using System.Collections.Generic;

namespace CalorieMonitor.Data.Interfaces
{
    public interface IDbFilterQueryHandler
    {
        string GenerateQuery(IFilter filter, out List<QueryParam> queryParameters);
    }
}
