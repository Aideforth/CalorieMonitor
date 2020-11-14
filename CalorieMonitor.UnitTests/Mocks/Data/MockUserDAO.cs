using CalorieMonitor.Core.Entities;
using CalorieMonitor.Core.Implementations;
using CalorieMonitor.Data.Interfaces;
using CalorieMonitor.UnitTests.Utilities;
using Moq;
using System.Threading.Tasks;

namespace CalorieMonitor.UnitTests.Mocks.Data
{
    public class MockUserDAO : MyMock<IUserDAO>
    {
        public void MockInsertAsync(User item, bool withException = false)
        {
            var setup = Setup(b => b.InsertAsync(It.Is<User>(n => n == item)));

            if (withException) setup.Throws(SqlExceptionUtil.Get()).Verifiable();
            else setup.Returns<User>(c => Task.FromResult(c)).Verifiable();

            verifications.Add(() =>
                Verify(b => b.InsertAsync(It.Is<User>(n => n == item)), 
                Times.Once()));
        }

        public void MockUpdateAsync(User item, bool withException = false)
        {
            var setup = Setup(b => b.UpdateAsync(It.Is<User>(n => n == item)));

            if (withException) setup.Throws(SqlExceptionUtil.Get()).Verifiable();
            else setup.Returns<User>(c => Task.FromResult(c)).Verifiable();

            verifications.Add(() =>
                Verify(b => b.UpdateAsync(It.Is<User>(n => n == item)), 
                Times.Once()));
        }

        public void MockGetAsync(long id, User item, bool withException = false)
        {
            var setup = Setup(b => b.GetAsync(It.Is<long>(n => n == id)));

            if (withException) setup.Throws(SqlExceptionUtil.Get()).Verifiable();
            else setup.ReturnsAsync(item).Verifiable();

            verifications.Add(() =>
                Verify(b => b.GetAsync(It.Is<long>(n => n == id)), 
                Times.Once()));
        }

        public void MockGetUserByEmailAsync(string email, User item, bool withException = false)
        {
            var setup = Setup(b => b.GetUserByEmailAsync(It.Is<string>(n => n == email)));

            if (withException) setup.Throws(SqlExceptionUtil.Get()).Verifiable();
            else setup.ReturnsAsync(item).Verifiable();

            verifications.Add(() =>
                Verify(b => b.GetUserByEmailAsync(It.Is<string>(n => n == email)), 
                Times.Once()));
        }

        public void MockGetUserByUserNameAsync(string userName, User item, bool withException = false)
        {
            var setup = Setup(b => b.GetUserByUserNameAsync(It.Is<string>(n => n == userName)));

            if (withException) setup.Throws(SqlExceptionUtil.Get()).Verifiable();
            else setup.ReturnsAsync(item).Verifiable();

            verifications.Add(() =>
                Verify(b => b.GetUserByUserNameAsync(It.Is<string>(n => n == userName)), 
                Times.Once()));
        }

        public void MockDeleteAsync(long id, bool withException = false)
        {
            var setup = Setup(b => b.DeleteAsync(It.Is<long>(n => n == id)));

            if (withException) setup.Throws(SqlExceptionUtil.Get()).Verifiable();
            else setup.ReturnsAsync(true).Verifiable();

            verifications.Add(() =>
                Verify(b => b.DeleteAsync(It.Is<long>(n => n == id)), 
                Times.Once()));
        }

        public void MockSearchUsersAsync(SearchFilter filter, int start, int limit, SearchResult<User> result, bool withException = false)
        {
            var setup = Setup(b => b.SearchUsersAsync(It.Is<SearchFilter>(n => n == filter), 
                It.Is<int>(n => n == start), 
                It.Is<int>(n => n == limit)));

            if (withException) setup.Throws(SqlExceptionUtil.Get()).Verifiable();
            else setup.ReturnsAsync(result).Verifiable();

            verifications.Add(() =>
                Verify(b => b.SearchUsersAsync(It.Is<SearchFilter>(n => n == filter), 
                It.Is<int>(n => n == start), 
                It.Is<int>(n => n == limit)), 
                Times.Once()));
        }
    }
}
