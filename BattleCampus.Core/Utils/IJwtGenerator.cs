using Microsoft.AspNetCore.Identity;

namespace BattleCampus.Core.Utils
{
    public interface IJwtGenerator
    {
        string CreateToken(IdentityUser user);
    }
}