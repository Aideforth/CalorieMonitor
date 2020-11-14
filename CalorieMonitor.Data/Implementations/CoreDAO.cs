using CalorieMonitor.Core.Entities;
using CalorieMonitor.Core.Implementations;
using CalorieMonitor.Data.Interfaces;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace CalorieMonitor.Data.Implementations
{
    public abstract class CoreDAO<T> : ICoreDAO<T> where T : Entity
    {
        protected readonly IDbConnectionProvider connectionProvider;
        protected readonly CRUDQueryObject queryObject;

        public CoreDAO(IDbConnectionProvider connectionProvider, CRUDQueryObject queryObject)
        {
            this.connectionProvider = connectionProvider ?? throw new ArgumentNullException("connectionProvider");
            this.queryObject = queryObject ?? throw new ArgumentNullException("queryObject");
        }
        public async Task<bool> DeleteAsync(long id)
        {
            IDbConnection connection = await connectionProvider.GetDbConnectionAsync();
            await connection.ExecuteAsync(queryObject.DeleteQuery, new Entity { Id = id });
            return true;
        }

        public async Task<T> GetAsync(long id)
        {
            IDbConnection connection = await connectionProvider.GetDbConnectionAsync();
            return await connection.QueryFirstOrDefaultAsync<T>(queryObject.SelectQuery, new Entity { Id = id });
        }

        public async Task<T> InsertAsync(T entity)
        {
            IDbConnection connection = await connectionProvider.GetDbConnectionAsync();
            entity.Id = await connection.ExecuteScalarAsync<long>(queryObject.InsertQuery, entity);
            return entity;
        }

        public async Task<T> UpdateAsync(T entity)
        {
            IDbConnection connection = await connectionProvider.GetDbConnectionAsync();
            await connection.ExecuteAsync(queryObject.UpdateQuery, entity);
            return entity;
        }

        protected DynamicParameters GenerateSearchParameters(List<QueryParam> queryParams, int startIndex, int limit)
        {
            DynamicParameters parameters = GenerateParameters(queryParams);
            parameters.Add("@Start", startIndex, DbType.Int32);
            parameters.Add("@Limit", limit, DbType.Int32);
            return parameters;
        }

        protected DynamicParameters GenerateParameters(List<QueryParam> queryParams)
        {
            DynamicParameters parameters = new DynamicParameters();
            queryParams.ForEach(v =>
            {
                parameters.Add(v.Name, v.Value, v.DbType);
            });
            return parameters;
        }
    }
}
