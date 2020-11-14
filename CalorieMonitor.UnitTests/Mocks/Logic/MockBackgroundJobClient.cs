using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using Moq;

namespace CalorieMonitor.UnitTests.Mocks.Logic
{
    public class MockBackgroundJobClient : MyMock<IBackgroundJobClient>
    {
        public void MockEnqueue()
        {
            verifications.Add(() => Verify(x => x.Create(
                It.Is<Job>(job => job.Method.Name == "ProcessMealItemsAsync"), 
                It.IsAny<EnqueuedState>()), 
                Times.Once));
        }
    }
}
