namespace Ustagram.Domain.DTOs;

public class PostResponseDTO
{
    public Guid Id { get; set; }
    public string PostType { get; set; }
    public string Text { get; set; }
    public string Description { get; set; }
    public int Price { get; set; }
    public string PhotoPath { get; set; }
    public string Date { get; set; }
    public int Likes { get; set; }
    public int Dislikes { get; set; }
    public Guid UserId { get; set; }
    public UserSummaryDTO User { get; set; } 
    public List<CommentResponseDTO> Comments { get; set; } = new List<CommentResponseDTO>();
    
    public string PhotoUrl { get; set; } 
}