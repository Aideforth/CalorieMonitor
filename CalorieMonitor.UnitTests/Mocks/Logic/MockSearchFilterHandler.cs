using CalorieMonitor.Core.Exceptions;
using CalorieMonitor.Core.Interfaces;
using CalorieMonitor.Logic.Interfaces;
using Moq;

namespace CalorieMonitor.UnitTests.Mocks.Logic
{
    public class MockSearchFilterHandler : MyMock<ISearchFilterHandler>
    {
        public void MockParse<T>(string filter, IFilter filterResponse, bool hasException = false)
        {
            var setup = Setup(v => v.Parse(filter, typeof(T)));

            if (hasException) setup.Throws(new InvalidFilterException("Invalid Syntax at ()")).Verifiable();
            else setup.Returns(filterResponse).Verifiable();

            verifications.Add(() => Verify(v => v.Parse(filter, typeof(T)), Times.Once()));
        }
    }
}
