using System.ComponentModel.DataAnnotations;

namespace Ustagram.Domain.DTOs;

public class CommentDTO
{
    [Required(ErrorMessage = "Content is required")]
    [StringLength(500, ErrorMessage = "Comment cannot exceed 500 characters")]
    public string Content { get; set; }

    [Required(ErrorMessage = "PostId is required")]
    public Guid PostId { get; set; }
}

public class CommentResponseDTO
{
    public Guid Id { get; set; }
    public string Content { get; set; }
    public string Date { get; set; }
    public Guid UserId { get; set; }
    public UserSummaryDTO User { get; set; }
    public Guid PostId { get; set; }
    public string UserPhotoUrl { get; set; }
}