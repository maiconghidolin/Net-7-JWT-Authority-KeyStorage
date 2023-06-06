using Domain.Entities;
using Domain.Interfaces.Services;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Configuration;
using NetDevPack.Security.Jwt.Core.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Core.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    private readonly IJwtService _jwtService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IJsonWebKeyStore _store;

    public TokenService(IConfiguration configuration, IJwtService jwtService, IHttpContextAccessor httpContextAccessor, IJsonWebKeyStore store)
    {
        _configuration = configuration;
        _jwtService = jwtService;
        _httpContextAccessor = httpContextAccessor;
        _store = store;
    }

    public async Task<string> CreateToken(User user)
    {
        var claims = GetUserClaims(user);
        var claimsIdentity = GetClaimsIdentity(claims);
        return await EncodeToken(claimsIdentity);
    }

    public async Task RevokeToken()
    {
        var key = await _store.GetCurrent();
        await _store.Revoke(key);
    }

    private List<Claim> GetUserClaims(User user)
    {
        List<Claim> claims = new()
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Key.ToString()), // Subject
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // JWT ID
            new Claim(JwtRegisteredClaimNames.Iat, ToUnixEpochDate(DateTime.UtcNow).ToString()), // Issued At
            new Claim(JwtRegisteredClaimNames.Nbf, ToUnixEpochDate(DateTime.UtcNow).ToString(), ClaimValueTypes.Integer64), // Issued At
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Email, user.Email)
        };

        return claims;
    }

    private ClaimsIdentity GetClaimsIdentity(List<Claim> claims)
    {
        var identityClaims = new ClaimsIdentity();
        identityClaims.AddClaims(claims);
        return identityClaims;
    }

    private async Task<string> EncodeToken(ClaimsIdentity claimsIdentity)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var issuer = $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host}";
        var signingCredentials = await _jwtService.GetCurrentSigningCredentials();
        var expires = DateTime.UtcNow.AddMinutes(60);

        var token = tokenHandler.CreateToken(new SecurityTokenDescriptor
        {
            Issuer = issuer,
            Subject = claimsIdentity,
            Expires = expires,
            IssuedAt = DateTime.UtcNow,
            NotBefore = DateTime.UtcNow,
            SigningCredentials = signingCredentials
        });

        return tokenHandler.WriteToken(token);
    }

    private static long ToUnixEpochDate(DateTime date)
    {
        return (long)Math.Round((date.ToUniversalTime() - new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero)).TotalSeconds);
    }

}
