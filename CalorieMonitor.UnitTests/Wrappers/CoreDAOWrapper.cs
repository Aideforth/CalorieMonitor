using CalorieMonitor.Core.Entities;
using CalorieMonitor.Core.Implementations;
using CalorieMonitor.Data.Implementations;
using CalorieMonitor.Data.Interfaces;
using Dapper;
using System.Collections.Generic;

namespace CalorieMonitor.UnitTests.Wrappers
{
    public class CoreDAOWrapper<T>: CoreDAO<T> where T:Entity
    {
        public CoreDAOWrapper(IDbConnectionProvider connectionProvider, CRUDQueryObject queryObject) 
            : base(connectionProvider, queryObject)
        {
        }
        public new DynamicParameters GenerateSearchParameters(List<QueryParam> queryParams, int startIndex, int limit)
        {
            return base.GenerateSearchParameters(queryParams, startIndex, limit);
        }

        public new DynamicParameters GenerateParameters(List<QueryParam> queryParams)
        {
            return base.GenerateParameters(queryParams);
        }
    }
}
