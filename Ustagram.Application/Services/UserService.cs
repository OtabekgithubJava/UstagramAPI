using Microsoft.EntityFrameworkCore;
using Ustagram.Application.Abstractions;
using Ustagram.Domain.DTOs;
using Ustagram.Domain.Model;
using Ustagram.Infrastructure.Persistance;
using BCrypt.Net;

namespace Ustagram.Application.Services;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _db;
    private readonly IFileService _fileService;

    public UserService(ApplicationDbContext db, IFileService fileService)
    {
        _db = db;
        _fileService = fileService;
    }

    public async Task<string> CreateUser(UserDTO userDto)
    {
        if (userDto.Password.Length < 8)
            throw new ArgumentException("Password must be at least 8 characters long.");
        
        string photoPath = "assets/default-profile.png";
    
        if (userDto.Photo != null)
        {
            var validExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(userDto.Photo.FileName).ToLowerInvariant();
            if (!validExtensions.Contains(extension))
                throw new ArgumentException("Only JPEG, PNG, or GIF files are allowed");
            
            photoPath = await _fileService.SaveFileAsync(userDto.Photo, "ProfilePhotos");
        }

        var newUser = new User
        {
            FullName = userDto.FullName,
            Username = userDto.Username,
            Password = BCrypt.Net.BCrypt.HashPassword(userDto.Password, workFactor: 11),
            Phone = userDto.Phone,
            Location = userDto.Location ?? "",
            PhotoPath = userDto.PhotoPath ?? "assets/default-profile.png",
            Dob = userDto.Dob ?? "",
            Status = userDto.Status ?? "",
            MasterType = userDto.MasterType ?? "",
            Bio = userDto.Bio ?? "",
            Experience = userDto.Experience,
            TelegramUrl = userDto.TelegramUrl ?? "",
            InstagramUrl = userDto.InstagramUrl ?? ""
        };

        await _db.Users.AddAsync(newUser);
        await _db.SaveChangesAsync();

        return "User Created!";
    }

    public async Task<UserResponseDTO> GetUserById(Guid userId)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) return null;

        return MapToUserResponseDTO(user);
    }

    public async Task<string> UpdateUser(Guid userId, UserDTO userDto)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user == null)
            throw new Exception("User not found");

        user.FullName = userDto.FullName;
        user.Username = userDto.Username;
        user.Phone = userDto.Phone;
        user.Location = userDto.Location;
        user.Dob = userDto.Dob;
        user.Status = userDto.Status;
        user.MasterType = userDto.MasterType;
        user.Bio = userDto.Bio;
        user.Experience = userDto.Experience;
        user.TelegramUrl = userDto.TelegramUrl;
        user.InstagramUrl = userDto.InstagramUrl;

        if (!string.IsNullOrEmpty(userDto.Password) && userDto.Password != "UNCHANGED")
        {
            user.Password = BCrypt.Net.BCrypt.HashPassword(userDto.Password);
        }

        if (userDto.PhotoPath != null)
        {
            user.PhotoPath = userDto.PhotoPath;
        }

        await _db.SaveChangesAsync();
        return "User updated successfully";
    }

    public async Task<string> DeleteUser(Guid id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return "User not found";

        _db.Users.Remove(user);
        await _db.SaveChangesAsync();
        return "User Deleted!";
    }

    public async Task<List<UserResponseDTO>> GetAllUsers()
    {
        var users = await _db.Users.ToListAsync();
        return users.Select(u => MapToUserResponseDTO(u)).ToList();
    }

    public async Task<UserResponseDTO> GetUserByUsername(string username)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Username == username);

        if (user == null) return null;

        return MapToUserResponseDTO(user);
    }

    public async Task<User> GetUserEntityByUsername(string username)
    {
        return await _db.Users
            .FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<List<UserResponseDTO>> SearchUsers(string term)
    {
        if (string.IsNullOrWhiteSpace(term))
            return new List<UserResponseDTO>();

        var users = await _db.Users
            .Where(u => u.Username.Contains(term) || u.FullName.Contains(term))
            .Take(10)
            .ToListAsync();

        return users.Select(u => MapToUserResponseDTO(u)).ToList();
    }
    
    public async Task<List<UserResponseDTO>> GetTopUsers()
    {
        return await _db.Users
            .OrderByDescending(u => u.Posts.Count)
            .ThenByDescending(u => u.Experience)
            .Take(10)
            .Select(u => new UserResponseDTO
            {
                Id = u.Id,
                FullName = u.FullName,
                Username = u.Username,
                PhotoPath = u.PhotoPath,
                MasterType = u.MasterType,
                Experience = u.Experience
            })
            .ToListAsync();
    }

    private UserResponseDTO MapToUserResponseDTO(User user)
    {
        return new UserResponseDTO
        {
            Id = user.Id,
            FullName = user.FullName,
            Username = user.Username,
            Phone = user.Phone,
            Location = user.Location,
            PhotoPath = user.PhotoPath,
            Dob = user.Dob,
            Status = user.Status,
            MasterType = user.MasterType,
            Bio = user.Bio,
            Experience = user.Experience,
            TelegramUrl = user.TelegramUrl,
            InstagramUrl = user.InstagramUrl,
            PhotoUrl = _fileService.GetFileUrl(user.PhotoPath)
        };
    }
}