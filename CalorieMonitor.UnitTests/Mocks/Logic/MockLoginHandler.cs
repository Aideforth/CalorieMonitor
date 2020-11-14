using CalorieMonitor.Core.Entities;
using CalorieMonitor.Logic.Interfaces;
using Moq;

namespace CalorieMonitor.UnitTests.Mocks.Logic
{
    public class MockLoginHandler : MyMock<ILoginHandler>
    {
        public void MockGetCalorieResultAsync(User user, string token)
        {
            Setup(v => v.LoginUserAndGenerateToken(user))
                .Returns(token).Verifiable();

            verifications.Add(() => Verify(v => v.LoginUserAndGenerateToken(user), Times.Once()));
        }
    }
}
