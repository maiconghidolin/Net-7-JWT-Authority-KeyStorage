using Domain.Entities;

namespace Domain.Interfaces.Services;

public interface ITokenService
{
    Task<string> CreateToken(User user);
    Task RevokeToken();
}
