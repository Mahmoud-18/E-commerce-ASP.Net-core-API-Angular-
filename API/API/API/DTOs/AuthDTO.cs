using API.Data.Models;

namespace API.DTOs
{
    public class AuthDTO
    {
        public bool IsAuthenticated { get; set; }
        public string Message { get; set; }      
        public User User { get; set; }
    }
}
