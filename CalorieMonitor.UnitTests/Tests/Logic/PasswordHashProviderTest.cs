using CalorieMonitor.Logic.Implementations;
using Xunit;

namespace CalorieMonitor.UnitTests.Tests.Logic
{
    public class PasswordHashProviderTest
    {
        [Fact]
        public void ComputeHash_ValidHash()
        {
            string hashedPassword = new PasswordHashProvider().ComputeHash("password");
            Assert.Equal(ComputeHash("password"), hashedPassword);
        }

        public static string ComputeHash(string password)
        {
            byte[] data = System.Text.Encoding.ASCII.GetBytes(password);
            data = new System.Security.Cryptography.SHA256Managed().ComputeHash(data);
            return System.Text.Encoding.ASCII.GetString(data);
        }
    }
}
