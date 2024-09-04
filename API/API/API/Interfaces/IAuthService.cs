using API.Data.Models;
using API.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace API.Interfaces
{
    public interface IAuthService
    {
        Task<AuthDTO> RegisterUserAsync(RegisterDTO register);
        Task<AuthDTO> CheckLoginAsync(LoginDTO login);
        Task<User> GetCurrentUser(ClaimsPrincipal userClaims);
        Task<RefreshToken> CreateOrUpdateRefreshToken(User user);
        Task<bool> IsValidRefreshTokenAsync(string userId, string token);
        string CreateJwtToken(User user);
    }
}
