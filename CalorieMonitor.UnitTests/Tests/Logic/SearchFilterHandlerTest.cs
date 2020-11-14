using CalorieMonitor.Core.Entities;
using CalorieMonitor.Core.Enums;
using CalorieMonitor.Core.Interfaces;
using CalorieMonitor.Logic.Implementations;
using System;
using System.Collections.Generic;
using Xunit;

namespace CalorieMonitor.UnitTests.Tests.Logic
{
    public class SearchFilterHandlerTest
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("     ")]
        public void GenerateQuery_EmptyInput_ReturnNull(string input)
        {
            //arrange

            //act
            IFilter searchFilter = new SearchFilterHandler().Parse(input, typeof(User));

            //assert
            Assert.Null(searchFilter);
        }

        [Theory]
        [InlineData("(id lt 20", ")")]
        [InlineData("id lt 20)", "(")]
        [InlineData("((id lt 20) OR (id lt 20)", ")")]
        [InlineData("((id lt 20) AND (id lt 20)))", "(")]
        [InlineData("((id lt 20))) AND (id lt 20", "(")]
        [InlineData("((EntryUser.FirstName eq '20))') AND (id lt 20", ")")]
        public void GenerateQuery_InvalidFilterUnclosedBracket_FormatException(string filter, string missing)
        {
            //arrange

            //act
            Exception exception = Record.Exception(() => new SearchFilterHandler().Parse(filter, typeof(MealEntry)));

            //assert
            Assert.IsType<FormatException>(exception);
            Assert.Equal($"Invalid filter field {missing} missing", exception.Message);
        }

        [Theory]
        [InlineData("     ")]
        [InlineData("id ")]
        [InlineData(" lt 20")]
        [InlineData("id lt 20 more")]
        [InlineData("id lt 20 more than 4")]
        public void GenerateQuery_InvalidFilterField_FormatException(string filterField)
        {
            //arrange
            string filter = $"(id lt 20) AND ({filterField})";

            //act
            Exception exception = Record.Exception(() => new SearchFilterHandler().Parse(filter, typeof(MealEntry)));

            //assert
            Assert.IsType<FormatException>(exception);
            Assert.Equal($"Invalid filter field syntax ({filterField})", exception.Message);
        }

        [Theory]
        [InlineData("Distanced")]
        [InlineData("EntryUser.Names")]
        public void GenerateQuery_InvalidPropertyName_FormatException(string propertyName)
        {
            //arrange
            string filter = $"(id lt 20) AND ({propertyName} eq value)";

            //act
            Exception exception = Record.Exception(() => new SearchFilterHandler().Parse(filter, typeof(MealEntry)));

            //assert
            Assert.IsType<FormatException>(exception);
            Assert.Equal($"Invalid filter field {propertyName}", exception.Message);
        }

        [Theory]
        [InlineData("Calories", "like", "10")]
        [InlineData("EntryUser.FirstName", "gt", "Name")]
        [InlineData("EntryUser.FirstName", "lt", "Name")]
        [InlineData("EntryUser.Id", "like", "20")]
        [InlineData("EntryDateTime", "like", "2019-02-12")]
        [InlineData("EntryUser.Role", "like", "Admin")]
        [InlineData("EntryUser.Role", "gt", "Admin")]
        [InlineData("EntryUser.Role", "lt", "Admin")]
        public void GenerateQuery_InvalidOperation_FormatException(string propertyName, string operation, string value)
        {
            //arrange
            string filter = $"(Calories gt 20) OR ({propertyName} {operation} {value})";

            //act
            Exception exception = Record.Exception(() => new SearchFilterHandler().Parse(filter, typeof(MealEntry)));

            //assert
            Assert.IsType<FormatException>(exception);
            Assert.Equal($"Invalid filter comparator {operation} for {propertyName}", exception.Message);
        }

        [Theory]
        [InlineData("Calories", "nice")]
        [InlineData("Calories", "12ef")]
        [InlineData("EntryUser.Id", "this")]
        [InlineData("EntryUser.Id", "14tu")]
        [InlineData("EntryDateTime", "this")]
        [InlineData("EntryDateTime", "12")]
        [InlineData("EntryUser.Role", "Admined")]
        public void GenerateQuery_InvalidFilterValue_FormatException(string propertyName, string value)
        {
            //arrange
            string filter = $"(Calories gt 20) OR ({propertyName} eq {value})";

            //act
            Exception exception = Record.Exception(() => new SearchFilterHandler().Parse(filter, typeof(MealEntry)));

            //assert
            Assert.IsType<FormatException>(exception);
            Assert.Equal($"Invalid value {value} for {propertyName}", exception.Message);
        }

        [Theory]
        [InlineData("(id lt 20) (id lt 20)", "(id lt 20) ^ (id lt 20)")]
        [InlineData("((id lt 20) (id lt 20)) OR (id eq 2)", "(id lt 20) ^ (id lt 20)")]
        [InlineData("(EntryUser.FirstName eq '20) (id lt 20))')  (id eq 2)", "(EntryUser.FirstName eq '20) (id lt 20))') ^ (id eq 2)")]
        public void GenerateQuery_InvalidFilterMissingComparator_FormatException(string filter, string errorMessage)
        {
            //arrange

            //act
            Exception exception = Record.Exception(() => new SearchFilterHandler().Parse(filter, typeof(MealEntry)));

            //assert
            Assert.IsType<FormatException>(exception);
            Assert.Equal($"Invalid filter comparator 'OR' or 'AND' missing (^) in '{errorMessage}'", exception.Message);
        }

        [Theory]
        [InlineData("(EntryUser.FirstName eq '') OR ()", "()")]
        [InlineData("(id lt 20) OR AND (id lt 20)", "AND")]
        [InlineData("((id lt 20) AND NICE (id lt 20)) OR (id eq 2)", "NICE")]
        [InlineData("(id lt 20) NEW (id lt 20)", "NEW")]
        [InlineData("(id mx 20) AND (id lt 20)", "mx")]
        [InlineData("(id eq 20) AND (id lt 20) OR", "OR")]
        public void GenerateQuery_InvalidFilterInvalidSyntax_FormatException(string filter, string syntax)
        {
            //arrange

            //act
            Exception exception = Record.Exception(() => new SearchFilterHandler().Parse(filter, typeof(MealEntry)));

            //assert
            Assert.IsType<FormatException>(exception);
            Assert.Equal($"Invalid filter syntax at '{syntax}'", exception.Message);
        }

        [Fact]
        public void GenerateQuery_Valid_ReturnsSearchFilter()
        {
            //arrange
            DateTime date1 = new DateTime(2019, 2, 20);
            DateTime date2 = new DateTime(2020, 1, 30);
            string filter = "((((EntryUser.FirstName like 'Valid ( () Name') OR (Calories gt 20)) aND (CALORIES lt 0)) or ((EntryDateTime lt '2019-02-20') And(EntryDateTime gt '2020-01-30'))) And(Id gt 20000)";
            string filterString = $"((((EntryUser.FirstName:Like:Valid ( () Name::String):Or:(MealEntry.Calories:GreaterThan:20::Double)):And:(MealEntry.Calories:LessThan:0::Double)):Or:((MealEntry.EntryDateTime:LessThan:{date1}::DateTime):And:(MealEntry.EntryDateTime:GreaterThan:{date2}::DateTime))):And:(MealEntry.Id:GreaterThan:20000::Int64)";

            //act
            IFilter searchFilter = new SearchFilterHandler().Parse(filter, typeof(MealEntry));

            //assert
            Assert.NotNull(searchFilter);

            Assert.Equal(filterString, searchFilter.FilterString());
        }
    }
}
