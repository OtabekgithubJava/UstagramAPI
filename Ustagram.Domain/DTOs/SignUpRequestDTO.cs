using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Ustagram.Domain.DTOs;

public class SignUpRequestDTO
{
    [Required]
    [StringLength(100)]
    public string FullName { get; set; }

    [Required]
    [StringLength(50, MinimumLength = 8)]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$", 
        ErrorMessage = "Parolda katta harf, kichik harf va raqam qatnashishi shart")]
    public string Password { get; set; }

    [Required]
    [StringLength(50)]
    [RegularExpression(@"^[a-zA-Z0-9_]{3,20}$", 
        ErrorMessage = "Username'da 3-20gacha belgi qatnashishi kerak (letters, numbers, underscores)")]
    public string Username { get; set; }

    [Required]
    [Phone]
    public string Phone { get; set; }

    [StringLength(100)]
    public string Location { get; set; }

    public IFormFile? Photo { get; set; }

    [StringLength(10)]
    public string Dob { get; set; }

    [StringLength(50)]
    public string Status { get; set; }

    [StringLength(50)]
    public string MasterType { get; set; }

    [StringLength(500)]
    public string Bio { get; set; }

    [Range(0, 100)]
    public int Experience { get; set; }

    [Url]
    public string TelegramUrl { get; set; }

    [Url]
    public string InstagramUrl { get; set; }
}