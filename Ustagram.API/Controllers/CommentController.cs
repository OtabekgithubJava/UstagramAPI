using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Ustagram.Application.Abstractions;
using Ustagram.Application.Services;
using Ustagram.Domain.DTOs;

namespace Ustagram.API.Controllers;

[Authorize]
[Route("api/[controller]/[action]")]
[ApiController]
public class CommentController : ControllerBase
{
    private readonly ICommentService _commentService;
    private readonly INotificationService _notificationService;

    public CommentController(ICommentService commentService, INotificationService notificationService)
    {
        _commentService = commentService;
        _notificationService = notificationService;
    }
    
    [HttpPost]
    public async Task<ActionResult<CommentResponseDTO>> CreateComment([FromBody] CommentDTO commentDto)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var comment = await _commentService.CreateComment(commentDto, userId);
            return CreatedAtAction(nameof(GetCommentById), new { commentId = comment.Id }, comment);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    [HttpPost("realtime")]
    public async Task<ActionResult<CommentResponseDTO>> CreateCommentRealtime(
        [FromBody] CommentDTO commentDto,
        [FromServices] IHubContext<CommentHub> hubContext)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        var comment = await _commentService.CreateComment(commentDto, userId);
        
        await hubContext.Clients.Group(commentDto.PostId.ToString())
            .SendAsync("ReceiveComment", comment);
        
        await _notificationService.NotifyCommentAsync(commentDto.PostId, userId, commentDto.Content);
    
        return CreatedAtAction(
            nameof(GetCommentById), 
            new { commentId = comment.Id }, 
            comment
        );
    }

    [HttpPut("{commentId}")]
    public async Task<ActionResult<string>> UpdateComment(Guid commentId, [FromForm] CommentDTO commentDTO)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        var result = await _commentService.UpdateComment(commentId, commentDTO, userId);
        

        return Ok(result);
    }

    [HttpDelete("{commentId}")]
    public async Task<ActionResult<string>> DeleteComment(Guid commentId)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        var result = await _commentService.DeleteComment(commentId, userId);

        return Ok(result);
    }

    [AllowAnonymous]
    [HttpGet("{commentId}")]
    public async Task<ActionResult<CommentResponseDTO>> GetCommentById(Guid commentId)
    {
        var comment = await _commentService.GetCommentById(commentId);
        if (comment == null)
            return NotFound("Comment not found");
        return Ok(comment);
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<List<CommentResponseDTO>>> GetAllComments()
    {
        var comments = await _commentService.GetAllComments();
        return Ok(comments);
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<List<CommentResponseDTO>>> GetCommentsByPost(Guid id)
    {
        var relatedComments = await _commentService.GetCommentsByPost(id);
        return Ok(relatedComments);
    }
}