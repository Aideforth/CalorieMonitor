using CalorieMonitor.Core.Entities;
using CalorieMonitor.Core.Implementations;
using CalorieMonitor.Logic.Interfaces;
using Moq;
using System;
using System.Threading.Tasks;

namespace CalorieMonitor.UnitTests.Mocks.Logic
{
    public class MockUserLogic : MyMock<IUserLogic>
    {
        public void MockCreateAsync(Exception exception = null)
        {
            var setup = Setup(b => b.CreateAsync(It.IsAny<User>()));

            if (exception != null) setup.Throws(exception).Verifiable();
            else setup.Returns<User>(c => Task.FromResult(c)).Verifiable();

            verifications.Add(() =>
                Verify(b => b.CreateAsync(It.IsAny<User>()), Times.Once())
                );
        }

        public void MockUpdateAsync(Exception exception = null)
        {
            var setup = Setup(b => b.UpdateAsync(It.IsAny<User>()));

            if (exception != null) setup.Throws(exception).Verifiable();
            else setup.Returns<User>(c => Task.FromResult(c)).Verifiable();

            verifications.Add(() =>
                Verify(b => b.UpdateAsync(It.IsAny<User>()), Times.Once())
                );
        }

        public void MockGetAsync(long id, User item, Exception exception = null, int timesInvoked = 1)
        {
            var setup = Setup(b => b.GetAsync(It.Is<long>(n => n == id)));

            if (exception != null) setup.Throws(exception).Verifiable();
            else setup.ReturnsAsync(item).Verifiable();

            verifications.Add(() =>
                Verify(b => b.GetAsync(It.Is<long>(n => n == id)), Times.Exactly(timesInvoked))
                );
        }

        public void MockDeleteAsync(long id, Exception exception = null)
        {
            var setup = Setup(b => b.DeleteAsync(It.Is<long>(n => n == id)));

            if (exception != null) setup.Throws(exception).Verifiable();
            else setup.ReturnsAsync(true).Verifiable();

            verifications.Add(() =>
                Verify(b => b.DeleteAsync(It.Is<long>(n => n == id)), Times.Once())
                );
        }

        public void MockSearchUsersAsync(SearchFilter filter, int start, int limit, SearchResult<User> result, Exception exception = null)
        {
            var setup = Setup(b => b.SearchUsersAsync(It.Is<SearchFilter>(n => n == filter), It.Is<int>(n => n == start), It.Is<int>(n => n == limit)));

            if (exception != null) setup.Throws(exception).Verifiable();
            else setup.ReturnsAsync(result).Verifiable();

            verifications.Add(() =>
                Verify(b => b.SearchUsersAsync(It.Is<SearchFilter>(n => n == filter), It.Is<int>(n => n == start), It.Is<int>(n => n == limit)), Times.Once())
                );
        }
        public void MockLoginUserAsync(string username, string password, string token, Exception exception = null)
        {
            var setup = Setup(b => b.LoginUserAsync(It.Is<string>(n => n == username), It.Is<string>(n => n == password)));

            if (exception != null) setup.Throws(exception).Verifiable();
            else setup.ReturnsAsync(token).Verifiable();

            verifications.Add(() =>
                Verify(b => b.LoginUserAsync(It.Is<string>(n => n == username), It.Is<string>(n => n == password)), Times.Once())
                );
        }

        public void MockChangeUserPasswordAsync(long id,string oldPassword, string newPassword, Exception exception = null)
        {
            var setup = Setup(b => b.ChangeUserPasswordAsync(It.Is<long>(n => n == id), 
                It.Is<string>(n => n == oldPassword),
                It.Is<string>(n => n == newPassword)));

            if (exception != null) setup.Throws(exception).Verifiable();
            else setup.Verifiable();

            verifications.Add(() =>
                Verify(b => b.ChangeUserPasswordAsync(It.Is<long>(n => n == id),
                It.Is<string>(n => n == oldPassword),
                It.Is<string>(n => n == newPassword)), Times.Once()));
        }

    }
}
