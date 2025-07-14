using Ustagram.Domain.DTOs;
using Ustagram.Domain.Model;

namespace Ustagram.Application.Abstractions;

public interface IFavouritesService
{
    Task<string> CreateFavourite(FavouriteDTO favouriteDto, Guid userId);
    Task<string> UpdateFavourite(Guid favouriteId, FavouriteDTO favouriteDto, Guid userId);
    Task<string> DeleteFavourite(Guid favouriteId, Guid userId);
    Task<FavouriteResponseDTO> GetFavouriteById(Guid favouriteId);
    Task<List<FavouriteResponseDTO>> GetAllFavourites();
    Task<List<FavouriteResponseDTO>> GetFavouritesByUser(Guid userId);
    Task<bool> IsPostFavoritedByUser(Guid postId, Guid userId);
    Task<int> GetFavoritesCountForPost(Guid postId);
    Task<List<FavouriteResponseDTO>> GetFavoritesForPost(Guid postId);
}