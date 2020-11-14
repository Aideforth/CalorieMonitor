namespace CalorieMonitor.Logic.Interfaces
{
    public interface IPasswordHashProvider
    {
        string ComputeHash(string password);
    }
}
