using CalorieMonitor.Core.Enums;
using CalorieMonitor.Core.Implementations;
using CalorieMonitor.Core.Interfaces;
using CalorieMonitor.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;

namespace CalorieMonitor.Data.Implementations
{
    public class SqlFilterQueryHandler : IDbFilterQueryHandler
    {
        readonly Dictionary<FilterComparison, string> filterComparisonStrings = new Dictionary<FilterComparison, string> {
            {FilterComparison.Equals, "=" },
            {FilterComparison.GreaterThan, ">" },
            {FilterComparison.LessThan, "<" },
            {FilterComparison.Like, "like" },
            {FilterComparison.NotEquals, "!=" },
        };
        readonly Dictionary<Type, DbType> typeToDbType = new Dictionary<Type, DbType> {
            {typeof(int), DbType.Int32 },
            {typeof(UserRole), DbType.Int32 },
            {typeof(string), DbType.String },
            {typeof(long), DbType.Int64 },
            {typeof(DateTime), DbType.DateTime },
        };
        readonly Dictionary<string, int> paramNameCounts = new Dictionary<string, int>();

        public string GenerateQuery(IFilter filter, out List<QueryParam> queryParameters)
        {
            if (filter == null)
            {
                queryParameters = new List<QueryParam>();
                return string.Empty;
            }
            string query = GenerateFilterQuery(filter, out queryParameters);
            query = $"where {query}";
            return query;
        }

        private string GenerateFieldQuery(FieldFilter filter, out QueryParam queryParameter)
        {
            string paramName = GetFieldParameterName(filter.FieldName);
            string query = $"{filter.FieldName} {filterComparisonStrings[filter.Comparision]} {paramName.Replace(".", "")}";

            typeToDbType.TryGetValue(filter.FieldValue.GetType(), out DbType type);

            queryParameter = new QueryParam { Name = paramName.Replace(".", ""), Value = filter.FieldValue, DbType = type };
            return query;
        }
        private string GenerateSearchFilterQuery(IFilter filter, out List<QueryParam> queryParameters)
        {
            var lHS = GenerateFilterQuery(filter.LeftHandFilter, out List<QueryParam> lHSQueryParam);
            var rHS = GenerateFilterQuery(filter.RightHandFilter, out List<QueryParam> rHSQueryParam);

            queryParameters = new List<QueryParam>();
            queryParameters.AddRange(lHSQueryParam);
            queryParameters.AddRange(rHSQueryParam);

            string query = $"{lHS} {filter.Operation} {rHS}";
            return query;
        }

        private string GenerateFilterQuery(IFilter filter, out List<QueryParam> queryParameters)
        {
            string query;
            switch (filter.Operation)
            {
                case FilterOperation.Field:
                    query = GenerateFieldQuery(filter as FieldFilter, out QueryParam queryParam);
                    queryParameters = new List<QueryParam> { queryParam };
                    break;

                default:
                    query = GenerateSearchFilterQuery(filter, out queryParameters);
                    break;
            }
            if (filter.HasBrackets) query = $"({query})";
            return query;
        }

        private string GetFieldParameterName(string fieldName)
        {
            string name = $"@{fieldName}";
            if (!paramNameCounts.TryGetValue(name, out int count))
            {
                paramNameCounts[name] = 1;
            }
            else
            {
                paramNameCounts[name]++;
                name = $"{name}{count}";
            }
            return name;
        }
    }
}
