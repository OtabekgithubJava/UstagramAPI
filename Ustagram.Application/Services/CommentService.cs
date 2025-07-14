using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Ustagram.Application.Abstractions;
using Ustagram.Domain.DTOs;
using Ustagram.Domain.Model;
using Ustagram.Infrastructure.Persistance;
using Microsoft.Extensions.Logging;

namespace Ustagram.Application.Services;

public class CommentService : ICommentService
{
    private readonly ApplicationDbContext _db;
    private readonly IFileService _fileService;
    private readonly IHubContext<CommentHub> _hubContext;
    private readonly ILogger<CommentService> _logger;

    public CommentService(
        ApplicationDbContext db,
        IFileService fileService,
        IHubContext<CommentHub> hubContext,
        ILogger<CommentService> logger)
    {
        _db = db;
        _fileService = fileService;
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task<CommentResponseDTO> CreateComment(CommentDTO commentDto, Guid userId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(commentDto.Content))
                throw new ArgumentException("Comment content cannot be empty");
            
            if (commentDto.PostId == Guid.Empty)
                throw new ArgumentException("Invalid Post ID");

            var comment = new Comment
            {
                Content = commentDto.Content.Trim(),
                Date = DateTime.UtcNow.ToString("O"),
                UserId = userId,
                PostId = commentDto.PostId
            };

            await _db.Comments.AddAsync(comment);
            await _db.SaveChangesAsync();
            
            var createdComment = await _db.Comments
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == comment.Id);

            if (createdComment == null)
                throw new Exception("Failed to retrieve created comment");

            var response = MapToCommentResponseDTO(createdComment);
            
            await _hubContext.Clients.Group(commentDto.PostId.ToString())
                .SendAsync("ReceiveComment", response);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating comment");
            throw;
        }
    }

    public async Task<CommentResponseDTO> UpdateComment(Guid commentId, CommentDTO commentDto, Guid userId)
    {
        try
        {
            var comment = await _db.Comments
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == commentId);

            if (comment == null)
                throw new KeyNotFoundException("Comment not found");

            if (comment.UserId != userId)
                throw new UnauthorizedAccessException("You can only edit your own comments");

            comment.Content = commentDto.Content.Trim();
            comment.Date = DateTime.UtcNow.ToString("O");
            await _db.SaveChangesAsync();

            return MapToCommentResponseDTO(comment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating comment {commentId}");
            throw;
        }
    }

    public async Task<bool> DeleteComment(Guid commentId, Guid userId)
    {
        try
        {
            var comment = await _db.Comments.FindAsync(commentId);
            if (comment == null)
                return false;

            if (comment.UserId != userId)
                throw new UnauthorizedAccessException("You can only delete your own comments");

            _db.Comments.Remove(comment);
            await _db.SaveChangesAsync();
            
            await _hubContext.Clients.Group(comment.PostId.ToString())
                .SendAsync("RemoveComment", commentId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting comment {commentId}");
            throw;
        }
    }

    public async Task<CommentResponseDTO> GetCommentById(Guid commentId)
    {
        try
        {
            var comment = await _db.Comments
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == commentId);

            return comment != null ? MapToCommentResponseDTO(comment) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting comment {commentId}");
            throw;
        }
    }

    public async Task<List<CommentResponseDTO>> GetAllComments()
    {
        try
        {
            return await _db.Comments
                .Include(c => c.User)
                .OrderByDescending(c => c.Date)
                .Select(c => MapToCommentResponseDTO(c))
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all comments");
            throw;
        }
    }

    public async Task<List<CommentResponseDTO>> GetCommentsByPost(Guid postId)
    {
        try
        {
            return await _db.Comments
                .Where(c => c.PostId == postId)
                .Include(c => c.User)
                .OrderByDescending(c => c.Date)
                .Select(c => MapToCommentResponseDTO(c))
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting comments for post {postId}");
            throw;
        }
    }

    public async Task<List<CommentResponseDTO>> GetRecentComments(int count = 5)
    {
        try
        {
            return await _db.Comments
                .Include(c => c.User)
                .OrderByDescending(c => c.Date)
                .Take(count)
                .Select(c => MapToCommentResponseDTO(c))
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting recent {count} comments");
            throw;
        }
    }

    private CommentResponseDTO MapToCommentResponseDTO(Comment comment)
    {
        return new CommentResponseDTO
        {
            Id = comment.Id,
            Content = comment.Content,
            Date = comment.Date,
            UserId = comment.UserId,
            PostId = comment.PostId,
            User = new UserSummaryDTO
            {
                Id = comment.User.Id,
                FullName = comment.User.FullName,
                Username = comment.User.Username,
                PhotoPath = comment.User.PhotoPath
            },
            UserPhotoUrl = _fileService.GetFileUrl(comment.User.PhotoPath)
        };
    }
}