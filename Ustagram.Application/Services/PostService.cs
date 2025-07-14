using Microsoft.EntityFrameworkCore;
using Ustagram.Application.Abstractions;
using Ustagram.Domain.DTOs;
using Ustagram.Domain.Model;
using Ustagram.Infrastructure.Persistance;
using System.Globalization;
using Microsoft.Extensions.Logging;

namespace Ustagram.Application.Services;

public class PostService : IPostService
{
    private readonly ApplicationDbContext _db;
    private readonly IFileService _fileService;
    private readonly ILogger<PostService> _logger;

    public PostService(ApplicationDbContext db, IFileService fileService, ILogger<PostService> logger)
    {
        _db = db;
        _fileService = fileService;
        _logger = logger;
    }

    public async Task<string> CreatePost(PostDTO postDto, Guid userId)
    {
        _logger.LogInformation("Creating post for user {UserId}: PostType={PostType}", userId, postDto.PostType);

        string photoPath = "assets/default-post.jpg"; 
    
        if (postDto.Attachment != null)
        {
            var validExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(postDto.Attachment.FileName).ToLowerInvariant();
            if (!validExtensions.Contains(extension))
                throw new ArgumentException("Only JPEG, PNG, or GIF files are allowed");
            
            photoPath = await _fileService.SaveFileAsync(postDto.Attachment, "PostPhotos");
        }

        var newPost = new Post
        {
            UserId = userId,
            PostType = postDto.PostType,
            Text = postDto.Text,
            Description = postDto.Description,
            Price = postDto.Price,
            PhotoPath = photoPath,
            Date = Convert.ToString(DateTime.UtcNow, CultureInfo.InvariantCulture),
            Likes = 0,
            Dislikes = 0,
        };

        await _db.Posts.AddAsync(newPost);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Post created: PostId={PostId}", newPost.Id);
        return "Post created";
    }

    public async Task<FileStream> GetPostAttachment(Guid postId)
    {
        var post = await _db.Posts.FindAsync(postId);
        if (post == null || string.IsNullOrEmpty(post.PhotoPath))
            return null;

        return _fileService.GetFile(post.PhotoPath);
    }

    public async Task<List<PostResponseDTO>> GetPostByUser(Guid userId)
    {
        var posts = await _db.Posts
            .Where(p => p.UserId == userId)
            .Include(p => p.User)
            .Include(p => p.Comments).ThenInclude(c => c.User)
            .ToListAsync();

        return posts.Select(post => MapToPostResponseDTO(post)).ToList();
    }

    public async Task<PostResponseDTO> GetPostById(Guid postId)
    {
        var post = await _db.Posts
            .Include(p => p.User)
            .Include(p => p.Comments).ThenInclude(c => c.User)
            .FirstOrDefaultAsync(p => p.Id == postId);

        if (post == null) return null;

        return MapToPostResponseDTO(post);
    }

    public async Task<string> UpdatePost(Guid postId, PostDTO postDto, Guid userId)
    {
        var post = await _db.Posts.FirstOrDefaultAsync(p => p.Id == postId);
        if (post == null)
            return "Post not found";

        if (post.UserId != userId)
            return "Unauthorized: You cannot edit this post";

        post.PostType = postDto.PostType;
        post.Text = postDto.Text;
        post.Description = postDto.Description;
        post.Price = postDto.Price;

        // if (postDto.Attachment != null)
        // {
        //     var newPhotoPath = await _fileService.SaveFileAsync(postDto.Attachment, "Posts");
        //     post.PhotoPath = newPhotoPath;
        // }
        // else if (!string.IsNullOrEmpty(postDto.PhotoPath))
        // {
        //     post.PhotoPath = postDto.PhotoPath;
        // }

        post.Date = DateTime.UtcNow.ToString("dd.MM.yyyy HH:mm");
        await _db.SaveChangesAsync();
        return "Post updated";
    }

    public async Task<string> DeletePost(Guid postId, Guid userId)
    {
        var post = await _db.Posts.FirstOrDefaultAsync(p => p.Id == postId);
        if (post == null)
            return "Post not found";

        if (post.UserId != userId)
            return "Unauthorized: You cannot delete this post";

        if (!string.IsNullOrEmpty(post.PhotoPath))
            _fileService.DeleteFile(post.PhotoPath);

        _db.Posts.Remove(post);
        await _db.SaveChangesAsync();
        return "Post deleted";
    }

    public async Task<List<PostResponseDTO>> GetAllPosts()
    {
        var posts = await _db.Posts
            .Include(p => p.User)
            .Include(p => p.Comments).ThenInclude(c => c.User)
            .ToListAsync();

        return posts.Select(post => MapToPostResponseDTO(post)).ToList();
    }

    public async Task<List<PostResponseDTO>> GetPostsLastFive()
    {
        var posts = await _db.Posts
            .Include(p => p.User)
            .Include(p => p.Comments).ThenInclude(c => c.User)
            .ToListAsync();

        return posts
            .OrderByDescending(p => DateTime.ParseExact(p.Date, "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture))
            .Take(5)
            .Select(post => MapToPostResponseDTO(post))
            .ToList();
    }

    public async Task<List<PostResponseDTO>> GetPostsTopFive()
    {
        var posts = await _db.Posts
            .Include(p => p.User)
            .Include(p => p.Comments).ThenInclude(c => c.User)
            .OrderByDescending(p => p.Likes)
            .Take(5)
            .ToListAsync();

        return posts.Select(post => MapToPostResponseDTO(post)).ToList();
    }

    public async Task<List<PostResponseDTO>> GetPostsIshCategory()
    {
        var posts = await _db.Posts
            .Where(p => p.PostType == "Ish")
            .Include(p => p.User)
            .Include(p => p.Comments).ThenInclude(c => c.User)
            .Take(5)
            .ToListAsync();

        return posts.Select(post => MapToPostResponseDTO(post)).ToList();
    }

    public async Task<List<PostResponseDTO>> GetPostsSotuvCategory()
    {
        var posts = await _db.Posts
            .Where(p => p.PostType == "Sotuv")
            .Include(p => p.User)
            .Include(p => p.Comments).ThenInclude(c => c.User)
            .Take(5)
            .ToListAsync();

        return posts.Select(post => MapToPostResponseDTO(post)).ToList();
    }

    public async Task<List<PostResponseDTO>> GetPostsReklamaCategory()
    {
        var posts = await _db.Posts
            .Where(p => p.PostType == "Reklama")
            .Include(p => p.User)
            .Include(p => p.Comments).ThenInclude(c => c.User)
            .Take(5)
            .ToListAsync();

        return posts.Select(post => MapToPostResponseDTO(post)).ToList();
    }

    public async Task<List<PostResponseDTO>> GetPostsByUserId(Guid userId)
    {
        var posts = await _db.Posts
            .Where(p => p.UserId == userId)
            .Include(p => p.User)
            .Include(p => p.Comments).ThenInclude(c => c.User)
            .ToListAsync();

        return posts.Select(post => MapToPostResponseDTO(post)).ToList();
    }

    private PostResponseDTO MapToPostResponseDTO(Post post)
    {
        return new PostResponseDTO
        {
            Id = post.Id,
            PostType = post.PostType,
            Text = post.Text,
            Description = post.Description,
            Price = post.Price,
            PhotoPath = post.PhotoPath,
            Date = post.Date,
            Likes = post.Likes,
            Dislikes = post.Dislikes,
            PhotoUrl = _fileService.GetFileUrl(post.PhotoPath),
            UserId = post.UserId,
            User = new UserSummaryDTO
            {
                Id = post.User.Id,
                FullName = post.User.FullName,
                Username = post.User.Username,
                PhotoPath = post.User.PhotoPath,
                PhotoUrl = _fileService.GetFileUrl(post.User.PhotoPath)
            },
            Comments = post.Comments.Select(c => new CommentResponseDTO
            {
                Id = c.Id,
                Content = c.Content,
                Date = c.Date,
                UserId = c.UserId,
                User = new UserSummaryDTO
                {
                    Id = c.User.Id,
                    FullName = c.User.FullName,
                    Username = c.User.Username,
                    PhotoPath = c.User.PhotoPath
                },
                PostId = c.PostId
            }).ToList()
        };
    }
}