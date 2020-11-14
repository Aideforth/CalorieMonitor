using Moq;
using System;
using System.Collections.Generic;

namespace CalorieMonitor.UnitTests.Mocks
{
    public class MyMock<T> : Mock<T> where T : class
    {
        protected List<Action> verifications;
        public MyMock()
        {
            verifications = new List<Action>();
        }
        public void RunVerification()
        {
            VerifyAll();
            verifications.ForEach(v => v());
            VerifyNoOtherCalls();
        }
    }
}
