using CalorieMonitor.Core.Implementations;
using CalorieMonitor.Core.Interfaces;
using CalorieMonitor.Data.Interfaces;
using Moq;
using System.Collections.Generic;

namespace CalorieMonitor.UnitTests.Mocks.Data
{
    public class MockDbFilterQueryHandler : Mock<IDbFilterQueryHandler>
    {
        delegate void methodDelegate(IFilter filter, out List<QueryParam> queryParams);

        public void MockGenerateQuery(IFilter filter, string query, List<QueryParam> queryParams)
        {

            Setup(v => v.GenerateQuery(It.Is<IFilter>(c => c == filter), out It.Ref<List<QueryParam>>.IsAny))
                .Callback(new methodDelegate((IFilter a, out List<QueryParam> qParams) =>
                {
                    qParams = queryParams;
                }))
                .Returns(query);
        }

        public void RunVerification()
        {
            VerifyAll();
            Verify(v => v.GenerateQuery(It.IsAny<IFilter>(), out It.Ref<List<QueryParam>>.IsAny));
            VerifyNoOtherCalls();
        }
    }
}
