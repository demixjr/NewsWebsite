using BLL.DTO;

namespace BLL.Interfaces
{
    public interface IUserService
    {
        Task<bool> AddUser(UserDTO userDTO);
        Task<UserDTO?> Authenticate(string username, string password);
        Task<UserDTO?> FindUserByUsername(string? username);
        Task<bool> DeleteUser(UserDTO userDTO);
        Task<UserDTO?> ChangePassword(UserDTO userDTO, string newPassword);
    }
}
