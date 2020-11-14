using CalorieMonitor.Logic.Interfaces;
using Moq;
using System;

namespace CalorieMonitor.UnitTests.Mocks.Logic
{
    public class MockLogManager : MyMock<ILogManager>
    {
        public void MockLogException<T>() where T : Exception
        {
            Setup(v => v.LogException(It.IsAny<T>())).Verifiable();
            verifications.Add(() =>
                Verify(b => b.LogException(It.IsAny<T>()), Times.Once())
                );
        }

        public void MockLogMessage(string message)
        {
            Setup(v => v.LogMessage(It.Is<string>(c => c == message))).Verifiable();
            verifications.Add(() =>
                Verify(b => b.LogMessage(It.Is<string>(c => c == message)), Times.Once())
                );
        }
    }
}
