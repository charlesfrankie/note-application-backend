using Azure.Core;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Win32;
using NoteApplication.Models.Entities;
using NoteApplication.Repositories;
using NoteApplication.Response;

namespace NoteApplication.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthenticationController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly UserRepository _userRepository;
        public AuthenticationController(IConfiguration config, UserRepository userRepository)
        {
            _config = config;
            _userRepository = userRepository;
        }

        [HttpPost("register", Name = "RegisterUser")]
        public ActionResult Register([FromBody] User user)
        {
            // Validate the user object
            if (user == null || string.IsNullOrEmpty(user.name) || string.IsNullOrEmpty(user.email) || string.IsNullOrEmpty(user.password))
            {
                return BadRequest("Invalid user data.");
            }

            int statusCode = _userRepository.CreateUser(user);
            return new UserResponse().JsonResponse(statusCode, "User registerd successfully!");
        }

        [HttpPost("login")]
        public ActionResult Login([FromBody] User user)
        {
            var token = "";
            if (user == null || string.IsNullOrEmpty(user.email) || string.IsNullOrEmpty(user.password))
            {
                return BadRequest("Invalid user data.");
            }

            var (statusCode, foundUser) = _userRepository.GetUser(user);
            //return new UserResponse().JsonResponse(statusCode, "User registerd successfully!");
            if(foundUser != null)
            {
                token = JwtTokenHelper.GenerateToken(foundUser.id.ToString(), _config);
            }
            return new UserResponse().JsonResponse(statusCode, "User login successfully!", new { data = foundUser, token });
        }
    }
}
