using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Ustagram.Domain.DTOs;

public class UserDTO
{
    public string FullName { get; set; }
    public string Password { get; set; }
    public string Username { get; set; }

    [Required]
    [Phone]
    public string Phone { get; set; }
    
    public string Location { get; set; }
    
    public string? PhotoPath { get; set; }
    
    public string Dob { get; set; }
    
    public string Status { get; set; }
    
    public string MasterType { get; set; }
    
    public string Bio { get; set; }
    
    public int Experience { get; set; }
    
    public string TelegramUrl { get; set; }
    
    public string InstagramUrl { get; set; }
    
    public IFormFile? Photo { get; set; } 
}


public class UserSummaryDTO
{
    public Guid Id { get; set; }
    public string FullName { get; set; }
    public string Username { get; set; }
    public string PhotoPath { get; set; }
    public string PhotoUrl { get; set; }
}



public class UserResponseDTO
{
    public Guid Id { get; set; }
    public string FullName { get; set; }
    public string Username { get; set; }
    public string Phone { get; set; }
    public string Location { get; set; }
    public string PhotoPath { get; set; }
    public string Dob { get; set; }
    public string Status { get; set; }
    public string MasterType { get; set; }
    public string Bio { get; set; }
    public int Experience { get; set; }
    public string TelegramUrl { get; set; }
    public string InstagramUrl { get; set; }
    
    public string PhotoUrl { get; set; } 
}


public class PublicUserDTO
{
    public Guid Id { get; set; }
    public string FullName { get; set; }
    public string Username { get; set; }
    public string PhotoPath { get; set; }
    public string Bio { get; set; }
}