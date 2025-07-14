using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ustagram.Application.Abstractions;
using Ustagram.Domain.DTOs;

namespace Ustagram.API.Controllers;

[Authorize]
[Route("api/[controller]/[action]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IUserService _service;
    private readonly ILogger<UserController> _logger;

    public UserController(IUserService service, ILogger<UserController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet("{userId}")]
    public async Task<ActionResult<UserResponseDTO>> GetUserById(Guid userId)
    {
        _logger.LogInformation("Fetching user: UserId={UserId}", userId);
        var user = await _service.GetUserById(userId);
        if (user == null)
        {
            _logger.LogWarning("User not found: UserId={UserId}", userId);
            return NotFound("User not found");
        }
        return Ok(user);
    }

    [HttpPut("{userId}")]
    public async Task<ActionResult<string>> UpdateUser(Guid userId, [FromForm] UserDTO userDto)
    {
        _logger.LogInformation("Update attempt: UserId={UserId}, Username={Username}", userId, userDto.Username);

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim != userId.ToString())
        {
            _logger.LogWarning("Unauthorized update attempt: UserId={UserId}, ClaimedId={ClaimedId}", userId, userIdClaim);
            return Unauthorized("You can only edit your own profile");
        }

        try
        {
            var result = await _service.UpdateUser(userId, userDto);
            _logger.LogInformation("User updated: UserId={UserId}, Username={Username}", userId, userDto.Username);
            return Ok(new {Message = result});
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Update failed: UserId={UserId}", userId);
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{userId}")]
    public async Task<ActionResult<string>> DeleteUser(Guid userId)
    {
        _logger.LogInformation("Delete attempt: UserId={UserId}", userId);

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim != userId.ToString())
        {
            _logger.LogWarning("Unauthorized delete attempt: UserId={UserId}, ClaimedId={ClaimedId}", userId, userIdClaim);
            return Unauthorized("You can only delete your own profile");
        }

        var result = await _service.DeleteUser(userId);
        if (result == "User not found")
        {
            _logger.LogWarning("User not found for deletion: UserId={UserId}", userId);
            return NotFound("User not found");
        }

        _logger.LogInformation("User deleted: UserId={UserId}", userId);
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<List<UserResponseDTO>>> GetAllUsers()
    {
        _logger.LogInformation("Fetching all users");
        var users = await _service.GetAllUsers();
        return Ok(users);
    }

    [AllowAnonymous]
    [HttpGet("{username}")]
    public async Task<ActionResult<UserResponseDTO>> GetUserByUsername(string username)
    {
        _logger.LogInformation("Fetching user by username: Username={Username}", username);
        var user = await _service.GetUserByUsername(username);
        if (user == null)
        {
            _logger.LogWarning("User not found: Username={Username}", username);
            return NotFound("User not found");
        }
        return Ok(user);
    }

    [AllowAnonymous]
    [HttpGet("search")]
    public async Task<ActionResult<List<UserResponseDTO>>> SearchUsers([FromQuery] string term)
    {
        _logger.LogInformation("Searching users: Term={Term}", term);
        var users = await _service.SearchUsers(term);
        return Ok(users);
    }
    
    [HttpGet("top")]
    public async Task<ActionResult<List<UserResponseDTO>>> GetTopUsers()
    {
        var users = await _service.GetTopUsers();
        return Ok(users);
    }
}