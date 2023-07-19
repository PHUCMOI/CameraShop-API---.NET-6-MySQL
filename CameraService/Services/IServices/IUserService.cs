using CameraCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CameraService.Services.IServices
{
    public interface IUserService
    {
        Task<IEnumerable<UserResponse>> GetAllUser();
        Task<UserResponse> GetIdAsync(int userId);
        Task<string> Create(UserRequest user);
        Task<bool> Update(UserRequest user, string userID, int id);
        Task<bool> DeleteAsync(int userId);
    }
}
