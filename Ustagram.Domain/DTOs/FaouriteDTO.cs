namespace Ustagram.Domain.DTOs;


public class FavouriteDTO
{
    public Guid PostId { get; set; }
}


public class FavouriteResponseDTO
{
    public Guid Id { get; set; }
    public Guid PostId { get; set; }
    public Guid UserId { get; set; }
    public UserSummaryDTO User { get; set; }
}