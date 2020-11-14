using CalorieMonitor.Logic.Interfaces;

namespace CalorieMonitor.Logic.Implementations
{
    public class PasswordHashProvider : IPasswordHashProvider
    {
        public string ComputeHash(string password)
        {
            byte[] data = System.Text.Encoding.ASCII.GetBytes(password);
            data = new System.Security.Cryptography.SHA256Managed().ComputeHash(data);
            return System.Text.Encoding.ASCII.GetString(data);
        }
    }
}
