using CalorieMonitor.Logic.Interfaces;
using Moq;

namespace CalorieMonitor.UnitTests.Mocks.Logic
{
    public class MockPasswordHashProvider : MyMock<IPasswordHashProvider>
    {
        public void MockComputeHash(string password, string hash)
        {
            Setup(v => v.ComputeHash(password)).Returns(hash);
            verifications.Add(() => Verify(v => v.ComputeHash(password), Times.Once));
        }
    }
}
