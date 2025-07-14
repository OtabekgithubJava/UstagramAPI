using Ustagram.Domain.DTOs;
using Ustagram.Domain.Model;

namespace Ustagram.Application.Abstractions;

public interface ICommentService
{
    Task<CommentResponseDTO> CreateComment(CommentDTO commentDto, Guid userId);
    Task<CommentResponseDTO> UpdateComment(Guid commentId, CommentDTO commentDto, Guid userId);
    Task<bool> DeleteComment(Guid commentId, Guid userId);
    Task<CommentResponseDTO> GetCommentById(Guid commentId);
    Task<List<CommentResponseDTO>> GetAllComments();
    Task<List<CommentResponseDTO>> GetCommentsByPost(Guid postId);
    Task<List<CommentResponseDTO>> GetRecentComments(int count = 5);
}