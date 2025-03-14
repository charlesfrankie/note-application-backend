using Azure.Core;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using NoteApplication.Models.Entities;
using NoteApplication.Repositories;

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

        [HttpPost("register")]
        public ActionResult Register([FromBody] User user)
        {
            // Validate the user object
            if (user == null || string.IsNullOrEmpty(user.name) || string.IsNullOrEmpty(user.email) || string.IsNullOrEmpty(user.password))
            {
                return BadRequest("Invalid user data.");
            }

            bool isCreated = _userRepository.CreateUser(user);

            if (!isCreated)
            {
                return StatusCode(500, "A problem happened while handling your request.");
            }

            // Return the token
            return Ok("User registerd successfully!");
        }

        [HttpPost("login")]
        public ActionResult Login([FromBody] User user)
        {
            // Validate the user object
            if (user == null || string.IsNullOrEmpty(user.email) || string.IsNullOrEmpty(user.password))
            {
                return BadRequest("Invalid user data.");
            }

            var foundUser = _userRepository.GetUser(user);

            if(foundUser == null)
            {
                return Unauthorized("Invalid username or password.");
            }

            // Generate JWT token
            var token = JwtTokenHelper.GenerateToken(foundUser.id.ToString(), _config);

            return Ok(new { id = foundUser.id, name = foundUser.name, email = foundUser.email, token = token });
        }
    }
}
