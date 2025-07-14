using Ustagram.Domain.DTOs;
using Ustagram.Domain.Model;

namespace Ustagram.Application.Abstractions;

public interface IUserService
{
    Task<string> CreateUser(UserDTO userDto);
    Task<UserResponseDTO> GetUserById(Guid userId);
    Task<string> UpdateUser(Guid userId, UserDTO userDto);
    Task<string> DeleteUser(Guid id);
    Task<List<UserResponseDTO>> GetAllUsers();
    Task<UserResponseDTO> GetUserByUsername(string username);
    Task<User> GetUserEntityByUsername(string username); 
    Task<List<UserResponseDTO>> SearchUsers(string term);
    Task<List<UserResponseDTO>> GetTopUsers();
}