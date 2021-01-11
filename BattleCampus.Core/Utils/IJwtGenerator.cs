namespace BattleCampus.Core.Utils
{
    public interface IJwtGenerator
    {
        string CreateToken(User user);
    }
}