using CalorieMonitor.Core.Entities;
using CalorieMonitor.Data.Interfaces;
using CalorieMonitor.UnitTests.Utilities;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CalorieMonitor.UnitTests.Mocks.Data
{
    public class MockMealItemDAO : MyMock<IMealItemDAO>
    {
        public void MockInsertAsync(MealItem item, bool withException = false)
        {
            var setup = Setup(b => b.InsertAsync(It.Is<MealItem>(n => n == item)));

            if (withException) setup.Throws(SqlExceptionUtil.Get()).Verifiable();
            else setup.Returns<MealItem>(c => Task.FromResult(c)).Verifiable();

            verifications.Add(() =>
                Verify(b => b.InsertAsync(It.Is<MealItem>(n => n == item)), 
                Times.Once()));
        }

        public void MockInsertAsyncValidateWithName(MealItem item)
        {
            Setup(b => b.InsertAsync(It.Is<MealItem>(n => n.Name == item.Name)))
                .Returns<MealItem>(c => Task.FromResult(c)).Verifiable();

            verifications.Add(() =>
                Verify(b => b.InsertAsync(It.Is<MealItem>(n => n.Name == item.Name)),
                Times.Once()));
        }

        public void MockUpdateAsync(MealItem item, bool withException = false)
        {
            var setup = Setup(b => b.UpdateAsync(It.Is<MealItem>(n => n == item)));

            if (withException) setup.Throws(SqlExceptionUtil.Get()).Verifiable();
            else setup.Returns<MealItem>(c => Task.FromResult(c)).Verifiable();

            verifications.Add(() =>
                Verify(b => b.UpdateAsync(It.Is<MealItem>(n => n == item)), 
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

        public void MockGetItemsByMealEntryIdAsync(long id, List<MealItem> result, bool withException = false)
        {
            var setup = Setup(b => b.GetItemsByMealEntryIdAsync(It.Is<long>(n => n == id)));

            if (withException) setup.Throws(SqlExceptionUtil.Get()).Verifiable();
            else setup.ReturnsAsync(result).Verifiable();

            verifications.Add(() =>
                Verify(b => b.GetItemsByMealEntryIdAsync(It.Is<long>(n => n == id)), 
                Times.Once()));
        }
    }
}
