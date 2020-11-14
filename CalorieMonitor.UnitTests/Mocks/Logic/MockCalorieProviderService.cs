using CalorieMonitor.Logic.Implementations;
using CalorieMonitor.Logic.Interfaces;
using Moq;

namespace CalorieMonitor.UnitTests.Mocks.Logic
{
    public class MockCalorieProviderService : MyMock<ICalorieProviderService>
    {
        public void MockGetCalorieResultAsync(string text, CalorieServiceResult result)
        {
            Setup(v => v.GetCalorieResultAsync(text))
                .ReturnsAsync(result).Verifiable();

            verifications.Add(() => Verify(v => v.GetCalorieResultAsync(text), Times.Once()));
        }
    }
}
