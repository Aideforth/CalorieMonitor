using CalorieMonitor.Core.Entities;

namespace CalorieMonitor.Logic.Interfaces
{
    public interface ILoginHandler
    {
        string LoginUserAndGenerateToken(User user);
    }
}
