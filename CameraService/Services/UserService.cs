using CameraAPI.Models;
using CameraAPI.Repositories;
using CameraCore.Models;
using CameraService.Services.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CameraService.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IAutoMapperService _autoMapperService;

        public UserService(IUserRepository userRepository, IAutoMapperService autoMapperService)
        {
            _userRepository = userRepository;
            _autoMapperService = autoMapperService;
        }
        public Task<bool> Create(UserRequest user, string userID)
        {
            if (user != null)
            {
                var user = new Category()
                {
                    UpdatedBy = Convert.ToInt16(userID),
                    UpdatedDate = DateTime.Now,
                    CreatedBy = Convert.ToInt16(userID),
                    CreatedDate = DateTime.Now,
                    IsDelete = false
                };

                await _userRepository.Create(user);

                //Lưu xuống db 
                var result = _unitOfWork.Save();

                if (result > 0)
                    return true;
                else
                    return true;
            }
            return false;
        }

        public Task<bool> DeleteAsync(int userId)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<UserResponse>> GetAllUser()
        {
            var userList = await _userRepository.GetAll();
            var userResponseList = _autoMapperService.MapList<User, UserResponse>(userList);
            return userResponseList;
        }

        public async Task<UserResponse> GetIdAsync(int userId)
        {
            var user = await _userRepository.GetById(userId);
            var userResponse = _autoMapperService.Map<User, UserResponse>(user);
            return userResponse;
        }

        public Task<bool> Update(UserResponse user, string userID, int id)
        {
            throw new NotImplementedException();
        }
    }
}
