using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Ustagram.Application.Abstractions;
using Ustagram.Domain.DTOs;

namespace Ustagram.API.Controllers;

[Authorize]
[Route("api/[controller]/[action]")]
[ApiController]
public class PostController : ControllerBase
{
    private readonly IPostService _service;
    private readonly IFileService _fileService;

    public PostController(IPostService service, IFileService fileService)
    {
        _service = service;
        _fileService = fileService;
    }

    [HttpPost]
    public async Task<ActionResult<string>> CreatePost([FromForm] PostDTO postDto)
    {
        Console.WriteLine($"Received PostDTO: PostType={postDto.PostType}, Text={postDto.Text}, Description={postDto.Description}, Price={postDto.Price}, PhotoPath={postDto.PhotoPath ?? "null"}");

        if (postDto.Attachment != null)
        {
            postDto.PhotoPath = await _fileService.SaveFileAsync(postDto.Attachment, "PostPhotos");
        }
        
        else if (string.IsNullOrEmpty(postDto.PhotoPath))
        {
            postDto.PhotoPath = $"https://picsum.photos/200/200?random={Guid.NewGuid()}";
        }
        
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(x => x.Value.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );
            return BadRequest(new { title = "Validation failed", errors });
        }
        

        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        var result = await _service.CreatePost(postDto, userId);
        return Ok(new {Message = result});
    }

    [HttpPut("{postId}")]
    public async Task<ActionResult<string>> UpdatePost(Guid postId, [FromForm] PostDTO postDto)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        var result = await _service.UpdatePost(postId, postDto, userId);

        if (result == "Unauthorized")
            return Unauthorized("You don't own this post");

        return Ok(result);
    }

    [HttpDelete("{postId}")]
    public async Task<ActionResult<string>> DeletePost(Guid postId)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        var result = await _service.DeletePost(postId, userId);

        if (result == "Unauthorized")
            return Unauthorized("You don't own this post");

        return Ok(result);
    }

    [AllowAnonymous]
    [HttpGet("{postId}")]
    public async Task<ActionResult<PostResponseDTO>> GetPostById(Guid postId)
    {
        var post = await _service.GetPostById(postId);
        if (post == null)
            return NotFound();

        if (!string.IsNullOrEmpty(post.PhotoPath))
        {
            post.PhotoUrl = Url.Action("GetPostAttachment", new { postId = post.Id })!;
        }

        return Ok(post);
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<List<PostResponseDTO>>> GetAllPosts()
    {
        var result = await _service.GetAllPosts();
        foreach (var post in result)
        {
            if (!string.IsNullOrEmpty(post.PhotoPath))
            {
                post.PhotoUrl = Url.Action("GetPostAttachment", new { postId = post.Id })!;
            }
        }
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpGet("{postId}/attachment")]
    public async Task<IActionResult> GetPostAttachment(Guid postId)
    {
        var fileStream = await _service.GetPostAttachment(postId);
        if (fileStream == null)
            return NotFound();

        var post = await _service.GetPostById(postId);
        var contentType = GetContentType(post.PhotoPath);

        return File(fileStream, contentType);
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<List<PostResponseDTO>>> GetPostsLastFive()
    {
        var result = await _service.GetPostsLastFive();
        foreach (var post in result)
        {
            if (!string.IsNullOrEmpty(post.PhotoPath))
            {
                post.PhotoUrl = Url.Action("GetPostAttachment", new { postId = post.Id })!;
            }
        }
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<List<PostResponseDTO>>> GetPostsTopFive()
    {
        var result = await _service.GetPostsTopFive();
        foreach (var post in result)
        {
            if (!string.IsNullOrEmpty(post.PhotoPath))
            {
                post.PhotoUrl = Url.Action("GetPostAttachment", new { postId = post.Id })!;
            }
        }
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<List<PostResponseDTO>>> GetPostsIshCategory()
    {
        var result = await _service.GetPostsIshCategory();
        foreach (var post in result)
        {
            if (!string.IsNullOrEmpty(post.PhotoPath))
            {
                post.PhotoUrl = Url.Action("GetPostAttachment", new { postId = post.Id })!;
            }
        }
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<List<PostResponseDTO>>> GetPostsSotuvCategory()
    {
        var result = await _service.GetPostsSotuvCategory();
        foreach (var post in result)
        {
            if (!string.IsNullOrEmpty(post.PhotoPath))
            {
                post.PhotoUrl = Url.Action("GetPostAttachment", new { postId = post.Id })!;
            }
        }
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<List<PostResponseDTO>>> GetPostsReklamaCategory()
    {
        var result = await _service.GetPostsReklamaCategory();
        foreach (var post in result)
        {
            if (!string.IsNullOrEmpty(post.PhotoPath))
            {
                post.PhotoUrl = Url.Action("GetPostAttachment", new { postId = post.Id })!;
            }
        }
        return Ok(result);
    }

    [Authorize]
    [HttpGet]
    public async Task<ActionResult<List<PostResponseDTO>>> GetPostByUser()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized("Invalid or missing user ID in token");

        var result = await _service.GetPostByUser(userId);
        foreach (var post in result)
        {
            if (!string.IsNullOrEmpty(post.PhotoPath))
            {
                post.PhotoUrl = Url.Action("GetPostAttachment", new { postId = post.Id })!;
            }
        }
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpGet("user/{userId}")]
    public async Task<ActionResult<List<PostResponseDTO>>> GetPostsByUserId(Guid userId)
    {
        var posts = await _service.GetPostsByUserId(userId);
        foreach (var post in posts)
        {
            if (!string.IsNullOrEmpty(post.PhotoPath))
            {
                post.PhotoUrl = Url.Action("GetPostAttachment", new { postId = post.Id })!;
            }
        }
        return Ok(posts);
    }

    private string GetContentType(string path)
    {
        if (string.IsNullOrEmpty(path))
            return "application/octet-stream";

        var ext = Path.GetExtension(path).ToLowerInvariant();
        return ext switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            _ => "application/octet-stream"
        };
    }
}