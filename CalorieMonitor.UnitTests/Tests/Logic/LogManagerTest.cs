using CalorieMonitor.Logic.Implementations;
using CalorieMonitor.UnitTests.Mocks.External;
using System;
using Xunit;

namespace CalorieMonitor.UnitTests.Tests.Logic
{
    public class LogManagerTest
    {
        readonly MockLogger mockLogger;
        readonly LogManager logManager;

        public LogManagerTest()
        {
            mockLogger = new MockLogger();
            logManager = new LogManager(mockLogger.Object);
        }

        [Fact]
        public void Constructor_NullILoggerArgument_ThrowsException()
        {
            Exception exception = Record.Exception(() => new LogManager(null));
            Assert.IsType<ArgumentNullException>(exception);
            Assert.Equal("logger", (exception as ArgumentNullException).ParamName);
        }

        [Fact]
        public void LogExceptionTest()
        {
            //Arrange
            Exception exception = new Exception("Message to you");
            string expectedLog = $"{DateTime.Now} {exception.Message} | {exception.InnerException?.Message} at {exception.StackTrace}";
            mockLogger.MockError(expectedLog);

            //Act
            logManager.LogException(exception);

            //Assert
            mockLogger.RunVerification();
        }

        [Fact]
        public void LogMessage()
        {
            //Arrange
            string expectedMessage = "Message to you";
            mockLogger.MockInformation(expectedMessage);

            //Act
            logManager.LogMessage(expectedMessage);

            //Assert
            mockLogger.RunVerification();
        }
    }
}
