using CalorieMonitor.Core.Enums;
using CalorieMonitor.Core.Implementations;
using CalorieMonitor.Data.Implementations;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Xunit;

namespace CalorieMonitor.UnitTests.Tests.Data
{
    public class SqlFilterQueryHandlerTest
    {
        List<QueryParam> parameters;
        readonly SearchFilter searchFilter;
        readonly SearchFilter searchFilter2;
        readonly DateTime currentTime;

        public SqlFilterQueryHandlerTest()
        {
            searchFilter = new SearchFilter
            {
                Operation = FilterOperation.Or,
                LeftHandFilter = new FieldFilter("Name", FilterComparison.Equals, "Name"),
                RightHandFilter = new FieldFilter("Age", FilterComparison.Equals, 10)
            };
            searchFilter2 = new SearchFilter
            {
                Operation = FilterOperation.And,
                LeftHandFilter = searchFilter,
                RightHandFilter = new FieldFilter("Distance", FilterComparison.Equals, 100)
            };

            currentTime = DateTime.UtcNow;
        }
        [Fact]
        public void GenerateQuery_Empty()
        {
            //act
            var query = new SqlFilterQueryHandler().GenerateQuery(null, out parameters);

            //assert
            Assert.Equal(String.Empty, query);
            Assert.Empty(parameters);
        }
        [Fact]
        public void GenerateQuery_validateSimple()
        {
            //act
            var query = new SqlFilterQueryHandler().GenerateQuery(searchFilter, out parameters);

            //assert
            Assert.Equal("where (Name = @Name) Or (Age = @Age)", query);
            ValidateFilter1Parameters(2);
        }

        [Fact]
        public void GenerateQuery_validateChainedSimple()
        {
            //act
            var query = new SqlFilterQueryHandler().GenerateQuery(searchFilter2, out parameters);

            //assert
            Assert.Equal("where (Name = @Name) Or (Age = @Age) And (Distance = @Distance)", query);
            ValidateFilter2Parameters(3);
        }

        [Fact]
        public void GenerateQuery_validateBracketSimple()
        {
            //arrange
            searchFilter.HasBrackets = true;

            //act
            var query = new SqlFilterQueryHandler().GenerateQuery(searchFilter2, out parameters);

            //assert
            Assert.Equal("where ((Name = @Name) Or (Age = @Age)) And (Distance = @Distance)", query);
            ValidateFilter2Parameters(3);
        }

        [Fact]
        public void GenerateQuery_validateBracketRecurringParameterNames()
        {
            //arrange
            SearchFilter searchFilter5 = SetComplicatedFilter();

            //act
            var query = new SqlFilterQueryHandler().GenerateQuery(searchFilter5, out parameters);

            //assert
            Assert.Equal("where ((((Name like @Name) Or (Age > @Age)) And (Distance = @Distance)) Or ((RecordDate < @RecordDate) And (RecordDate > @RecordDate1))) And (Distance > @Distance1)", query);
            ValidateFilter2Parameters(6); 
            Assert.Equal(1, parameters.Count(c => c.Name == "@RecordDate" && (DateTime)c.Value == currentTime.AddDays(-1) && c.DbType == DbType.DateTime));
            Assert.Equal(1, parameters.Count(c => c.Name == "@RecordDate1" && (DateTime)c.Value == currentTime && c.DbType == DbType.DateTime));
            Assert.Equal(1, parameters.Count(c => c.Name == "@Distance1" && (int)c.Value == 101 && c.DbType == DbType.Int32));
        }

        private SearchFilter SetComplicatedFilter()
        {
            searchFilter.LeftHandFilter = new FieldFilter("Name", FilterComparison.Like, "Name");
            searchFilter.RightHandFilter = new FieldFilter("Age", FilterComparison.GreaterThan, 10);
            searchFilter.HasBrackets = true;
            searchFilter2.HasBrackets = true;

            SearchFilter searchFilter3 = new SearchFilter
            {
                Operation = FilterOperation.And,
                LeftHandFilter = new FieldFilter("RecordDate", FilterComparison.LessThan, currentTime.AddDays(-1)),
                RightHandFilter = new FieldFilter("RecordDate", FilterComparison.GreaterThan, currentTime),
                HasBrackets = true
            };

            SearchFilter searchFilter4 = new SearchFilter
            {
                Operation = FilterOperation.Or,
                LeftHandFilter = searchFilter2,
                RightHandFilter = searchFilter3,
                HasBrackets = true
            };

            SearchFilter searchFilter5 = new SearchFilter
            {
                Operation = FilterOperation.And,
                LeftHandFilter = searchFilter4,
                RightHandFilter = new FieldFilter("Distance", FilterComparison.GreaterThan, 101)
            };
            return searchFilter5;
        }

        private void ValidateFilter1Parameters(int count)
        {
            Assert.Equal(count, parameters.Count);
            Assert.Equal(1, parameters.Count(c => c.Name == "@Name" && (string)c.Value == "Name" && c.DbType == DbType.String));
            Assert.Equal(1, parameters.Count(c => c.Name == "@Age" && (int)c.Value == 10 && c.DbType == DbType.Int32));
        }
        private void ValidateFilter2Parameters(int count)
        {
            ValidateFilter1Parameters(count);
            Assert.Equal(1, parameters.Count(c => c.Name == "@Distance" && (int)c.Value == 100 && c.DbType == DbType.Int32));
        }
    }
}
