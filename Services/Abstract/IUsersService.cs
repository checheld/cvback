using Domain;

namespace Services.Abstract
{
    public interface IUsersService
    {
        Task<UserDTO> AddUser(UserDTO user);
        Task DeleteUserById(int id);
        Task<UserDTO> GetUserById(int id);
        Task<List<UserDTO>> GetUsersBySearch(string search);
        Task<UserDTO> UpdateUser(UserDTO user);
        Task<List<UserDTO>> GetAllUsers();
    }
}