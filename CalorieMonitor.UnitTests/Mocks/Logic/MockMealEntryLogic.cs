using CalorieMonitor.Core.Entities;
using CalorieMonitor.Core.Implementations;
using CalorieMonitor.Core.Interfaces;
using CalorieMonitor.Logic.Interfaces;
using Moq;
using System;
using System.Threading.Tasks;

namespace CalorieMonitor.UnitTests.Mocks.Logic
{
    public class MockMealEntryLogic : MyMock<IMealEntryLogic>
    {
        public void MockSaveAsync(Exception exception = null)
        {
            var setup = Setup(b => b.SaveAsync(It.IsAny<MealEntry>()));

            if (exception != null) setup.Throws(exception).Verifiable();
            else setup.Returns<MealEntry>(c => Task.FromResult(c)).Verifiable();

            verifications.Add(() =>
                Verify(b => b.SaveAsync(It.IsAny<MealEntry>()), Times.Once())
                );
        }

        public void MockUpdateAsync(MealEntry item, Exception exception = null)
        {
            var setup = Setup(b => b.UpdateAsync(It.Is<MealEntry>(n => n == item)));

            if (exception != null) setup.Throws(exception).Verifiable();
            else setup.Returns<MealEntry>(c => Task.FromResult(c)).Verifiable();

            verifications.Add(() =>
                Verify(b => b.UpdateAsync(It.Is<MealEntry>(n => n == item)), Times.Once())
                );
        }

        public void MockGetAsync(long id, MealEntry item, Exception exception = null)
        {
            var setup = Setup(b => b.GetAsync(It.Is<long>(n => n == id)));

            if (exception != null) setup.Throws(exception).Verifiable();
            else setup.ReturnsAsync(item).Verifiable();

            verifications.Add(() =>
                Verify(b => b.GetAsync(It.Is<long>(n => n == id)), Times.Once())
                );
        }

        public void MockDeleteAsync(long id, Exception exception = null)
        {
            var setup = Setup(b => b.DeleteAsync(It.Is<long>(n => n == id)));

            if (exception != null) setup.Throws(exception).Verifiable();
            else setup.ReturnsAsync(true).Verifiable();

            verifications.Add(() =>
                Verify(b => b.DeleteAsync(It.Is<long>(n => n == id)), Times.Once())
                );
        }

        public void MockSearchEntriesAsync(SearchFilter filter, int start, int limit, SearchResult<MealEntry> result, Exception exception = null)
        {
            var setup = Setup(b => b.SearchEntriesAsync(It.Is<SearchFilter>(n => n == filter), It.Is<int>(n => n == start), It.Is<int>(n => n == limit)));

            if (exception != null) setup.Throws(exception).Verifiable();
            else setup.ReturnsAsync(result).Verifiable();

            verifications.Add(() =>
                Verify(b => b.SearchEntriesAsync(It.Is<SearchFilter>(n => n == filter), It.Is<int>(n => n == start), It.Is<int>(n => n == limit)), Times.Once())
                );
        }

        public void MockSearchEntriesWithAnyFilterAsync(int start, int limit, SearchResult<MealEntry> result)
        {
            Setup(b => b.SearchEntriesAsync(It.IsAny<IFilter>(),
               It.Is<int>(n => n == start),
               It.Is<int>(n => n == limit)))
               .ReturnsAsync(result).Verifiable();

            verifications.Add(() =>
                Verify(b => b.SearchEntriesAsync(It.IsAny<IFilter>(),
                It.Is<int>(n => n == start),
                It.Is<int>(n => n == limit)), Times.Once()));
        }
    }
}
