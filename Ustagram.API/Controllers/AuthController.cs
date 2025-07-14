using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ustagram.Application.Abstractions;
using Ustagram.Domain.DTOs;
using Ustagram.Domain.Model;
using BCrypt.Net;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Ustagram.Application.Services;

namespace Ustagram.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IFileService _fileService;
        private readonly JwtService _jwtService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IUserService userService, IFileService fileService, JwtService jwtService, ILogger<AuthController> logger)
        {
            _userService = userService;
            _fileService = fileService;
            _jwtService = jwtService;
            _logger = logger;
        }

        [HttpPost("signup")]
        public async Task<IActionResult> SignUp([FromForm] UserDTO request)
        {
            _logger.LogInformation("Signup attempt: Username={Username}, FullName={FullName}", request.Username, request.FullName);

            if (request.Photo != null)
            {
                request.PhotoPath = await _fileService.SaveFileAsync(request.Photo, "ProfilePhotos");
            }
            
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                _logger.LogWarning("Invalid ModelState for signup: Username={Username}, Errors={Errors}", request.Username, string.Join("; ", errors));
                return BadRequest(new { error = "Validation failed", details = errors });
            }

            if (string.IsNullOrEmpty(request.FullName) || string.IsNullOrEmpty(request.Username) ||
                string.IsNullOrEmpty(request.Password) || string.IsNullOrEmpty(request.Phone))
            {
                _logger.LogWarning("Missing required fields: Username={Username}", request.Username);
                return BadRequest(new { error = "FullName, Username, Password, and Phone are required." });
            }
            
            var passwordRegex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.{8,})");
            if (!passwordRegex.IsMatch(request.Password))
            {
                _logger.LogWarning("Weak password: Username={Username}", request.Username);
                return BadRequest(new { error = "Password must be at least 8 characters with uppercase, lowercase, and numbers." });
            }

            var existingUser = await _userService.GetUserByUsername(request.Username);
            if (existingUser != null)
            {
                _logger.LogWarning("Username already exists: {Username}", request.Username);
                return Conflict(new { error = "Username already exists." });
            }
            

            try
            {
                var userDto = new UserDTO
                {
                    FullName = SanitizeInput(request.FullName),
                    Password = BCrypt.Net.BCrypt.HashPassword(request.Password), // Hash password
                    Username = SanitizeInput(request.Username),
                    Phone = request.Phone,
                    Location = SanitizeInput(request.Location),
                    PhotoPath = request.PhotoPath,
                    Dob = request.Dob,
                    Status = request.Status,
                    MasterType = request.MasterType,
                    Bio = SanitizeInput(request.Bio),
                    Experience = request.Experience,
                    TelegramUrl = request.TelegramUrl,
                    InstagramUrl = request.InstagramUrl
                };

                // if (request.Photo != null)
                // {
                //     if (request.Photo.Length > 5 * 1024 * 1024)
                //     {
                //         _logger.LogWarning("Photo too large: Username={Username}, Size={Size}", request.Username, request.Photo.Length);
                //         return BadRequest(new { error = "Photo size must be less than 5MB." });
                //     }
                //
                //     if (!new[] { "image/jpeg", "image/png" }.Contains(request.Photo.ContentType))
                //     {
                //         _logger.LogWarning("Invalid photo type: Username={Username}, ContentType={ContentType}", request.Username, request.Photo.ContentType);
                //         return BadRequest(new { error = "Only JPEG or PNG photos are allowed." });
                //     }
                //
                //     userDto.PhotoPath = await _fileService.SaveFileAsync(request.Photo, "Photos");
                //     _logger.LogInformation("Photo uploaded for user: {PhotoPath}", userDto.PhotoPath);
                // }

                var result = await _userService.CreateUser(userDto);
                var user = await _userService.GetUserByUsername(userDto.Username);
                var token = _jwtService.GenerateToken(user.Id.ToString());
                _logger.LogInformation("User created: Username={Username}, Id={UserId}", user.Username, user.Id);
                return Ok(new { message = result, token, user });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Signup failed: {Message}", ex.Message);
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Signup failed for Username={Username}", request.Username);
                return StatusCode(500, new { error = "Ro'yhatdan o'tishda xatolik." });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromForm] LoginRequest request)
        {
            _logger.LogInformation("Login attempt: Username={Username}", request.Username);

            if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
            {
                _logger.LogWarning("Missing username or password");
                return BadRequest(new { error = "Username and Password are required." });
            }

            try
            {
                var userEntity = await _userService.GetUserEntityByUsername(request.Username);
                if (userEntity == null)
                {
                    _logger.LogWarning("User not found: Username={Username}", request.Username);
                    return Unauthorized(new { error = "Invalid username or password" });
                }

                // bool passwordValid = BCrypt.Net.BCrypt.Verify(request.Password, userEntity.Password);
                // if (!passwordValid)
                // {
                //     _logger.LogWarning("Invalid password for Username={Username}", request.Username);
                //     return Unauthorized(new { error = "Invalid username or password" });
                // }

                var user = await _userService.GetUserByUsername(request.Username);
                var token = _jwtService.GenerateToken(user.Id.ToString());
        
                _logger.LogInformation("Login successful: Username={Username}, UserId={UserId}", user.Username, user.Id);
                return Ok(new { 
                    token, 
                    user = new {
                        user.Id,
                        user.FullName,
                        user.Username,
                        user.PhotoPath,
                        user.Status
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login error for Username={Username}", request.Username);
                return StatusCode(500, new { error = "An error occurred during login" });
            }
        }

        private string SanitizeInput(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            return input.Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;").Replace("'", "&#39;");
        }
    }

    public class LoginRequest
    {
        [FromForm(Name = "username")]
        public string Username { get; set; }

        [FromForm(Name = "password")]
        public string Password { get; set; }
    }
}