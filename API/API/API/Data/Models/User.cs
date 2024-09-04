using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace API.Data.Models;

public class User : IdentityUser
{
    public DateTime LastLoginTime { get; set; }


    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
