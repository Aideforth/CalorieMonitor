using CalorieMonitor.Core.Entities;
using CalorieMonitor.Core.Implementations;
using CalorieMonitor.Data.Interfaces;
using CalorieMonitor.UnitTests.Utilities;
using Moq;
using System;
using System.Threading.Tasks;

namespace CalorieMonitor.UnitTests.Mocks.Data
{
    public class MockMealEntryDAO : MyMock<IMealEntryDAO>
    {
        public void MockInsertAsync(MealEntry item, bool withException = false)
        {
            var setup = Setup(b => b.InsertAsync(It.Is<MealEntry>(n => n == item)));

            if (withException) setup.Throws(SqlExceptionUtil.Get()).Verifiable();
            else setup.Returns<MealEntry>(c => Task.FromResult(c)).Verifiable();

            verifications.Add(() =>
                Verify(b => b.InsertAsync(It.Is<MealEntry>(n => n == item)),
                Times.Once()));
        }

        public void MockUpdateAsync(MealEntry item, bool withException = false)
        {
            var setup = Setup(b => b.UpdateAsync(It.Is<MealEntry>(n => n == item)));

            if (withException) setup.Throws(SqlExceptionUtil.Get()).Verifiable();
            else setup.Returns<MealEntry>(c => Task.FromResult(c)).Verifiable();

            verifications.Add(() =>
                Verify(b => b.UpdateAsync(It.Is<MealEntry>(n => n == item)),
                Times.Once()));
        }

        public void MockGetAsync(long id, MealEntry item, bool withException = false)
        {
            var setup = Setup(b => b.GetAsync(It.Is<long>(n => n == id)));

            if (withException) setup.Throws(SqlExceptionUtil.Get()).Verifiable();
            else setup.ReturnsAsync(item).Verifiable();

            verifications.Add(() =>
                Verify(b => b.GetAsync(It.Is<long>(n => n == id)),
                Times.Once()));
        }

        public void MockGetEntryAsync(long id, MealEntry item, bool withException = false)
        {
            var setup = Setup(b => b.GetEntryAsync(It.Is<long>(n => n == id)));

            if (withException) setup.Throws(SqlExceptionUtil.Get()).Verifiable();
            else setup.ReturnsAsync(item).Verifiable();

            verifications.Add(() =>
                Verify(b => b.GetEntryAsync(It.Is<long>(n => n == id)),
                Times.Once()));
        }

        public void MockDeleteAsync(long id, bool withException = false)
        {
            var setup = Setup(b => b.DeleteAsync(It.Is<long>(n => n == id)));

            if (withException) setup.Throws(SqlExceptionUtil.Get()).Verifiable();
            else setup.ReturnsAsync(true).Verifiable();

            verifications.Add(() =>
                Verify(b => b.DeleteAsync(It.Is<long>(n => n == id)),
                Times.Once()));
        }

        public void MockSearchEntriesAsync(SearchFilter filter, int start, int limit, SearchResult<MealEntry> result, bool withException = false)
        {
            var setup = Setup(b => b.SearchEntriesAsync(It.Is<SearchFilter>(n => n == filter),
                It.Is<int>(n => n == start),
                It.Is<int>(n => n == limit)));

            if (withException) setup.Throws(SqlExceptionUtil.Get()).Verifiable();
            else setup.ReturnsAsync(result).Verifiable();

            verifications.Add(() =>
                Verify(b => b.SearchEntriesAsync(It.Is<SearchFilter>(n => n == filter),
                It.Is<int>(n => n == start),
                It.Is<int>(n => n == limit)),
                Times.Once()));
        }

        public void MockGetTotalCaloriesForUserInCurrentDateAsync(long id, DateTime date, double calories, bool withException = false)
        {
            var setup = Setup(b => b.GetTotalCaloriesForUserInCurrentDateAsync(It.Is<long>(n => n == id),
                It.Is<DateTime>(n => n == date)));

            if (withException) setup.Throws(SqlExceptionUtil.Get()).Verifiable();
            else setup.ReturnsAsync(calories).Verifiable();

            verifications.Add(() =>
                Verify(b => b.GetTotalCaloriesForUserInCurrentDateAsync(It.Is<long>(n => n == id),
                It.Is<DateTime>(n => n == date)),
                Times.Once()));
        }
    }
}
