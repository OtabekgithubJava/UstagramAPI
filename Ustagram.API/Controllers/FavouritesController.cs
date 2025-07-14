using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ustagram.Application.Abstractions;
using Ustagram.Domain.DTOs;

namespace Ustagram.API.Controllers;

[Authorize]
[Route("api/[controller]/[action]")]
[ApiController]
public class FavouritesController : ControllerBase
{
    private readonly IFavouritesService _service;
    private readonly INotificationService _notificationService;

    public FavouritesController(IFavouritesService service, INotificationService notificationService)
    {
        _service = service;
        _notificationService = notificationService;
    }

    [HttpPost]
    public async Task<ActionResult<string>> CreateFavourite([FromForm] FavouriteDTO favouriteDto)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        var result = await _service.CreateFavourite(favouriteDto, userId);
        
        if (result == "Data Created!")
        {
            await _notificationService.NotifyLikeAsync(favouriteDto.PostId, userId);
        }
        
        return Ok(result);
    }

    [HttpPut("{favouriteId}")]
    public async Task<ActionResult<string>> UpdateFavourite(Guid favouriteId, [FromForm] FavouriteDTO favouriteDto)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        var result = await _service.UpdateFavourite(favouriteId, favouriteDto, userId);

        if (result == "Unauthorized")
            return Unauthorized("You don't own this favourite");

        return Ok(result);
    }

    [HttpDelete("{favouriteId}")]
    public async Task<ActionResult<string>> DeleteFavourite(Guid favouriteId)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        var result = await _service.DeleteFavourite(favouriteId, userId);

        if (result == "Unauthorized")
            return Unauthorized("You don't own this favourite");

        return Ok(result);
    }

    [AllowAnonymous]
    [HttpGet("{favouriteId}")]
    public async Task<ActionResult<FavouriteResponseDTO>> GetFavouriteById(Guid favouriteId)
    {
        var favourite = await _service.GetFavouriteById(favouriteId);
        if (favourite == null)
            return NotFound("Favourite not found");
        return Ok(favourite);
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<List<FavouriteResponseDTO>>> GetFavourites()
    {
        var favourites = await _service.GetAllFavourites();
        return Ok(favourites);
    }

    [HttpGet("my")]
    public async Task<ActionResult<List<FavouriteResponseDTO>>> GetMyFavourites()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized("Invalid or missing user ID");

        var result = await _service.GetFavouritesByUser(userId);
        return Ok(result);
    }
    
    
    [HttpGet("post/{postId}/count")]
    public async Task<ActionResult<int>> GetFavoritesCountForPost(Guid postId)
    {
        var count = await _service.GetFavoritesCountForPost(postId);
        return Ok(count);
    }

    [HttpGet("post/{postId}")]
    public async Task<ActionResult<List<FavouriteResponseDTO>>> GetFavoritesForPost(Guid postId)
    {
        var favorites = await _service.GetFavoritesForPost(postId);
        return Ok(favorites);
    }

    [HttpGet("check/{postId}")]
    public async Task<ActionResult<bool>> IsPostFavorited(Guid postId)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        var isFavorited = await _service.IsPostFavoritedByUser(postId, userId);
        return Ok(isFavorited);
    }
}