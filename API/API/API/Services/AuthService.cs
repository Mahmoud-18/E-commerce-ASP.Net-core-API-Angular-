using API.Data.Context;
using API.Data.Models;
using API.DTOs;
using API.Helper;
using API.Helpers;
using API.Interfaces;
using Azure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace API.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly ECommerceDBContext _dBContext;
        private readonly JWT _jwt;

        public AuthService(UserManager<User> userManager, IOptions<JWT> jwt, ECommerceDBContext dBContext)
        {
            _userManager = userManager;
            _jwt = jwt.Value;
            _dBContext = dBContext;
        }
        public async Task<User> GetCurrentUser(ClaimsPrincipal userClaims)
        {
            string email = userClaims.FindFirstValue(ClaimTypes.Email);

            var user = await _userManager.FindByEmailAsync(email);

            user.LastLoginTime = DateTime.UtcNow;

            await _userManager.UpdateAsync(user);

            return user;
        }

        public async Task<AuthDTO> CheckLoginAsync(LoginDTO login)
        {
            var user = await _userManager.FindByNameAsync(login.UserName);

            if (user is null || !await _userManager.CheckPasswordAsync(user, login.Password))
                return new AuthDTO { Message = "Username or Password is incorrect!", IsAuthenticated = false };

            user.LastLoginTime = DateTime.UtcNow;

            await _userManager.UpdateAsync(user);
           
            return new AuthDTO
            {
                IsAuthenticated = true,    
                User = user,
            };
        }

        public async Task<AuthDTO> RegisterUserAsync(RegisterDTO register)
        {
            if (await _userManager.FindByEmailAsync(register.Email) is not null)
                return new AuthDTO { Message = "Email is already registered", IsAuthenticated = false };

            if (await _userManager.FindByNameAsync(register.UserName) is not null)
                return new AuthDTO { Message = "Username is already registered", IsAuthenticated = false };

            var user = new User
            {
                UserName = register.UserName,
                Email = register.Email,
                LastLoginTime = DateTime.UtcNow,
            };

            var result = await _userManager.CreateAsync(user, register.Password);

            if (!result.Succeeded)
            {
                var errors = string.Empty;

                foreach (var error in result.Errors)
                {
                    errors += error.Description + ", ";
                }

                return new AuthDTO { Message = errors, IsAuthenticated = false };
            }      
            
            return new AuthDTO
            {             
                IsAuthenticated = true,
                Message = "Account Created Successfully",
                User = user,
            };
        }

        public string CreateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("uid", user.Id)
            };

            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
            var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

            var jwtSecurityToken = new JwtSecurityToken(
                issuer: _jwt.Issuer,
                audience: _jwt.Audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(_jwt.ExpiresInMinutes),
                signingCredentials: signingCredentials);

            var tokenHandler = new JwtSecurityTokenHandler();

            return tokenHandler.WriteToken(jwtSecurityToken);
        }

        public async Task<RefreshToken> CreateOrUpdateRefreshToken(User user)
        {
            var token = new byte[32];
            using var randomNumberGenerator = RandomNumberGenerator.Create();
            randomNumberGenerator.GetBytes(token);

            var refreshToken = new RefreshToken()
            {
                Token = Convert.ToBase64String(token),
                User = user,
                DateExpiresUtc = DateTime.UtcNow.AddDays(_jwt.RefreshTokenExpiresInDays),
                UserId = user.Id,
            };

            var existingRefreshToken = await _dBContext.RefreshTokens.SingleOrDefaultAsync(x => x.UserId == user.Id);
            if (existingRefreshToken != null)
            {
                existingRefreshToken.Token = refreshToken.Token;
                existingRefreshToken.DateCreatedUtc = refreshToken.DateCreatedUtc;
                existingRefreshToken.DateExpiresUtc = refreshToken.DateExpiresUtc;
            }
            else
            {
                user.RefreshTokens.Add(refreshToken);
            }

            await _dBContext.SaveChangesAsync();

            return refreshToken;
        }

        public async Task<bool> IsValidRefreshTokenAsync(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token)) return false;

            var fetchedRefreshToken = await _dBContext.RefreshTokens
                .FirstOrDefaultAsync(x => x.UserId == userId && x.Token == token);

            if (fetchedRefreshToken == null) return false;
            if (fetchedRefreshToken.IsExpired) return false;

            return true;
        }

    }
}
