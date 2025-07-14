using Ustagram.Domain.DTOs;
using Ustagram.Domain.Model;

namespace Ustagram.Application.Abstractions;

public interface IPostService
{
    Task<string> CreatePost(PostDTO postDto, Guid userId);
    Task<FileStream> GetPostAttachment(Guid postId);
    Task<List<PostResponseDTO>> GetPostByUser(Guid id);
    Task<PostResponseDTO> GetPostById(Guid postId);
    Task<string> UpdatePost(Guid postId, PostDTO postDto, Guid userId);
    Task<string> DeletePost(Guid postId, Guid userId);
    Task<List<PostResponseDTO>> GetAllPosts();
    Task<List<PostResponseDTO>> GetPostsLastFive();
    Task<List<PostResponseDTO>> GetPostsTopFive();
    Task<List<PostResponseDTO>> GetPostsIshCategory();
    Task<List<PostResponseDTO>> GetPostsSotuvCategory();
    Task<List<PostResponseDTO>> GetPostsReklamaCategory();
    Task<List<PostResponseDTO>> GetPostsByUserId(Guid userId);
}