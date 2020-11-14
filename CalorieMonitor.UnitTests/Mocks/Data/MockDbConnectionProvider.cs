using CalorieMonitor.Data.Interfaces;
using CalorieMonitor.UnitTests.Mocks.External;
using Moq;

namespace CalorieMonitor.UnitTests.Mocks.Data
{
    public class MockDbConnectionProvider : Mock<IDbConnectionProvider>
    {
        public void MockGetDbConnectionAsync(MockDbConnection connection)
        {
            Setup(v => v.GetDbConnectionAsync()).
                ReturnsAsync(connection.Object).Verifiable();
        }

        public void RunVerification(int getConnectionCalls = 1)
        {
            Verify(v => v.GetDbConnectionAsync(), 
                Times.Exactly(getConnectionCalls));
            VerifyAll();
            VerifyNoOtherCalls();
        }
    }
}
