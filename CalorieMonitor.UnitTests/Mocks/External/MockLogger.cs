using Moq;
using Serilog;

namespace CalorieMonitor.UnitTests.Mocks.External
{
    public class MockLogger : MyMock<ILogger>
    {
        public void MockInformation(string message)
        {
            Setup(b => b.Information(It.Is<string>(n => n == message)))
                .Verifiable();

            verifications.Add(() =>
                Verify(b => b.Information(It.Is<string>(n => n == message)),
                Times.Once()));
        }

        public void MockError(string message)
        {
            Setup(b => b.Error(It.Is<string>(n => n == message)))
                .Verifiable();

            verifications.Add(() =>
                Verify(b => b.Error(It.Is<string>(n => n == message)),
                Times.Once()));
        }
    }
}
