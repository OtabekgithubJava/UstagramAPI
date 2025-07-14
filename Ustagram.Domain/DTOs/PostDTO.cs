using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Ustagram.Domain.DTOs;

public class PostDTO
{
    [Required]
    public string PostType { get; set; }
    [Required]
    public string Text { get; set; }
    public string Description { get; set; }
    [Required]
    public int Price { get; set; }
    public string? PhotoPath { get; set; } 
    public IFormFile Attachment { get; set; } 
}