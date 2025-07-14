using Microsoft.EntityFrameworkCore;
using Ustagram.Application.Abstractions;
using Ustagram.Domain.DTOs;
using Ustagram.Domain.Model;
using Ustagram.Infrastructure.Persistance;

namespace Ustagram.Application.Services;

public class FavouritesService : IFavouritesService
{
    private readonly ApplicationDbContext _db;
    private readonly IPostService _postService;
    private readonly INotificationService _notificationService;

    public FavouritesService(ApplicationDbContext db, IPostService postService, INotificationService notificationService)
    {
        _db = db;
        _postService = postService;
        _notificationService = notificationService;
    }
    
    
    public async Task<bool> IsPostFavoritedByUser(Guid postId, Guid userId)
    {
        return await _db.Favourites
            .AnyAsync(f => f.PostId == postId && f.UserId == userId);
    }

    public async Task<int> GetFavoritesCountForPost(Guid postId)
    {
        return await _db.Favourites
            .CountAsync(f => f.PostId == postId);
    }

    public async Task<List<FavouriteResponseDTO>> GetFavoritesForPost(Guid postId)
    {
        return await _db.Favourites
            .Include(f => f.User)
            .Where(f => f.PostId == postId)
            .Select(f => new FavouriteResponseDTO
            {
                Id = f.Id,
                PostId = f.PostId,
                UserId = f.UserId,
                User = new UserSummaryDTO
                {
                    Id = f.User.Id,
                    FullName = f.User.FullName,
                    Username = f.User.Username,
                    PhotoPath = f.User.PhotoPath
                }
            })
            .ToListAsync();
    }

    public async Task<string> CreateFavourite(FavouriteDTO favouriteDto, Guid userId)
    {
        var newFavourite = new Favourites
        {
            PostId = favouriteDto.PostId,
            UserId = userId
        };
        _db.Favourites.Add(newFavourite);
        await _db.SaveChangesAsync();
        return "Favourite created";
    }

    public async Task<string> UpdateFavourite(Guid favouriteId, FavouriteDTO favouriteDto, Guid userId)
    {
        var favourite = await _db.Favourites.FindAsync(favouriteId);
        if (favourite == null) return "Favourite not found";

        if (favourite.UserId != userId) return "Unauthorized";

        favourite.PostId = favouriteDto.PostId;
        await _db.SaveChangesAsync();
        return "Favourite updated";
    }

    public async Task<string> DeleteFavourite(Guid favouriteId, Guid userId)
    {
        var favourite = await _db.Favourites.FindAsync(favouriteId);
        if (favourite == null) return "Favourite not found";

        if (favourite.UserId != userId) return "Unauthorized";

        _db.Favourites.Remove(favourite);
        await _db.SaveChangesAsync();
        return "Favourite deleted";
    }

    public async Task<FavouriteResponseDTO> GetFavouriteById(Guid favouriteId)
    {
        var favourite = await _db.Favourites
            .Include(f => f.User)
            .FirstOrDefaultAsync(f => f.Id == favouriteId);

        if (favourite == null) return null;

        return new FavouriteResponseDTO
        {
            Id = favourite.Id,
            PostId = favourite.PostId,
            UserId = favourite.UserId,
            User = new UserSummaryDTO
            {
                Id = favourite.User.Id,
                FullName = favourite.User.FullName,
                Username = favourite.User.Username,
                PhotoPath = favourite.User.PhotoPath
            }
        };
    }

    public async Task<List<FavouriteResponseDTO>> GetAllFavourites()
    {
        var favourites = await _db.Favourites
            .Include(f => f.User)
            .ToListAsync();

        return favourites.Select(f => new FavouriteResponseDTO
        {
            Id = f.Id,
            PostId = f.PostId,
            UserId = f.UserId,
            User = new UserSummaryDTO
            {
                Id = f.User.Id,
                FullName = f.User.FullName,
                Username = f.User.Username,
                PhotoPath = f.User.PhotoPath
            }
        }).ToList();
    }

    public async Task<List<FavouriteResponseDTO>> GetFavouritesByUser(Guid userId)
    {
        var favourites = await _db.Favourites
            .Include(f => f.User)
            .Where(f => f.UserId == userId)
            .ToListAsync();

        return favourites.Select(f => new FavouriteResponseDTO
        {
            Id = f.Id,
            PostId = f.PostId,
            UserId = f.UserId,
            User = new UserSummaryDTO
            {
                Id = f.User.Id,
                FullName = f.User.FullName,
                Username = f.User.Username,
                PhotoPath = f.User.PhotoPath
            }
        }).ToList();
    }
}