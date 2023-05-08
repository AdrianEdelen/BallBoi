using Tofu.BallBoi.Abstractions.DataTransferObjects;

namespace Tofu.BallBoi.Abstractions.Interfaces;
public interface IUserRepository
{
    Task<UserDTO>? GetUserByIdAsync(int id);
    Task<IEnumerable<UserDTO>> GetAllUsersAsync();
    Task<UserDTO> AddUserAsync(UserDTO user);
    Task<UserDTO> UpdateUserAsync(UserDTO user);
    Task<bool> DeleteUserAsync(int id);
}