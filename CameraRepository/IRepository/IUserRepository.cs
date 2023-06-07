using CameraAPI.Models;

namespace CameraAPI.Repositories
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<List<User>> GetUserList();
        bool Delete(int userId);
    }
}
