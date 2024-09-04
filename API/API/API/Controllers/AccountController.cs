using API.Data.Models;
using API.DTOs;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AccountController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync(RegisterDTO register)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.RegisterUserAsync(register);

            if (!result.IsAuthenticated)
                return BadRequest(result.Message);


            var refreshToken = await _authService.CreateOrUpdateRefreshToken(result.User);

            AddRefreshTokenToCookie(refreshToken);

            var token = _authService.CreateJwtToken(result.User);

            var user = new UserDTO
            {
                Token = token,
                UserName = result.User.UserName,
                Email = result.User.Email
            };

            return Ok(user);
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync(LoginDTO login)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.CheckLoginAsync(login);

            if (!result.IsAuthenticated)
                return BadRequest(result.Message);

            var refreshToken = await _authService.CreateOrUpdateRefreshToken(result.User);

            AddRefreshTokenToCookie(refreshToken);

            var token = _authService.CreateJwtToken(result.User);

            var user = new UserDTO
            {
                Token = token,
                UserName = result.User.UserName,
                Email = result.User.Email
            };

            return Ok(user);
        }


        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpGet]  
        public async Task<IActionResult> RefreshPage()
        {
            var user = await _authService.GetCurrentUser(User);

            var refreshToken = await _authService.CreateOrUpdateRefreshToken(user);
            AddRefreshTokenToCookie(refreshToken);

            var jwttoken = _authService.CreateJwtToken(user);

            var userDTO = new UserDTO
            {
                Token = jwttoken,
                UserName = user.UserName,
                Email = user.Email
            };

            return Ok(userDTO);
        }

        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken()
        {
            var user = await _authService.GetCurrentUser(User);
            var token = Request.Cookies["identityAppRefreshToken"];

            if (await _authService.IsValidRefreshTokenAsync(user.Id, token))
            {
                var refreshToken = await _authService.CreateOrUpdateRefreshToken(user);
                AddRefreshTokenToCookie(refreshToken);

                var jwttoken = _authService.CreateJwtToken(user);

                var userDTO = new UserDTO
                {
                    Token = jwttoken,
                    UserName = user.UserName,
                    Email = user.Email
                };

                return Ok(userDTO);
            }

            return Unauthorized("Invalid or expired token, please try to login");

        }



        private void AddRefreshTokenToCookie(RefreshToken refreshToken)
        {
            var cookieOptions = new CookieOptions
            {
                Expires = refreshToken.DateExpiresUtc,
                IsEssential = true,
                HttpOnly = true,
                Secure = true,
                Path = "/",             
                SameSite = SameSiteMode.None,
                
            };

            Response.Cookies.Append("identityAppRefreshToken", refreshToken.Token, cookieOptions);
        }

    }
}
