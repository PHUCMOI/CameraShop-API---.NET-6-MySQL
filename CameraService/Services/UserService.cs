using CameraAPI.Models;
using CameraAPI.Repositories;
using CameraCore.Models;
using CameraService.Services.IServices;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CameraService.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IAutoMapperService _autoMapperService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly CameraAPIdbContext _context;

        public UserService(IUserRepository userRepository, IAutoMapperService autoMapperService, IUnitOfWork unitOfWork, CameraAPIdbContext context)
        {
            _userRepository = userRepository;
            _autoMapperService = autoMapperService;
            _unitOfWork = unitOfWork;
            _context = context;
        }
        public async Task<string> Create(UserRequest userRequest)
        {
            if (userRequest != null)
            {
                if (await CheckUserNameExist(userRequest.Username))
                    return "UserName is exist!";
                if (await CheckEmailExist(userRequest.Email))
                    return "Email is exist!";

                var pass = CheckPasswordStrength(userRequest.Password);
                userRequest.Password = PasswordHasher.HashPassword(userRequest.Password);

                var user = new User()
                {
                    Username = userRequest.Username,
                    Password = userRequest.Password,
                    Role = "Customer",
                    Email = userRequest.Email,
                    PhoneNumber = userRequest.PhoneNumber,
                    Status = "Active",
                    //UpdatedBy = Convert.ToInt16(userID),
                    UpdatedBy = 1,
                    UpdatedDate = DateTime.Now,
                    CreatedBy = 1,
                    //CreatedBy = Convert.ToInt16(userID),
                    CreatedDate = DateTime.Now,
                    IsDelete = false
                };

                await _userRepository.Create(user);

                //Lưu xuống db 
                var result = _unitOfWork.Save();

                if (result > 0)
                    return "Success";
            }
            return "Failed";
        }


        public Task<bool> DeleteAsync(int userId)
        {
            if (userId > 0)
            {
                var user = _userRepository.Delete(userId);
                if (user)
                {
                    var result = _unitOfWork.Save();
                    if (result == 0) return Task.FromResult(true);
                }
            }
            return Task.FromResult(false);
        }

        public async Task<IEnumerable<UserResponse>> GetAllUser()
        {
            var userList = await _userRepository.GetUserList();
            var userResponseList = _autoMapperService.MapList<User, UserResponse>(userList);
            return userResponseList;
        }

        public async Task<UserResponse> GetIdAsync(int userId)
        {
            var user = await _userRepository.GetById(userId);
            if (user.IsDelete == true)
            {
                return null;
            }
            var userResponse = _autoMapperService.Map<User, UserResponse>(user);
            return userResponse;
        }

        public async Task<bool> Update(UserRequest user, string userID, int id)
        {
            if (user != null)
            {
                var userDetail = await _userRepository.GetById(id);
                if (user != null)
                {
                    userDetail.Username = user.Username;
                    //userDetail.Role = user.Role;
                    userDetail.Password = user.Password;
                    userDetail.Email = user.Email;
                    userDetail.PhoneNumber = user.PhoneNumber;
                    //userDetail.Status = user.Status;
                    userDetail.IsDelete = false;
                    userDetail.UpdatedDate = DateTime.Now;
                    userDetail.CreatedDate = userDetail.CreatedDate;
                    userDetail.CreatedBy = userDetail.CreatedBy;
                    userDetail.UpdatedBy = Convert.ToInt16(userID);

                    _userRepository.Update(userDetail);
                    var result = _unitOfWork.Save();
                    if (result > 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private Task<bool> CheckUserNameExist(string username)
        {
            return _context.Users.AnyAsync(x => x.Username == username);
        }

        private Task<bool> CheckEmailExist(string email)
        {
            return _context.Users.AnyAsync(x => x.Email == email);
        }

        private string CheckPasswordStrength(string password)
        {
            StringBuilder sb = new StringBuilder();
            if(password.Length < 8) 
                sb.Append("Minium password length should be 8" + Environment.NewLine);
            if (Regex.IsMatch(password, "[a-z]") && Regex.IsMatch(password, "[A-Z]") && Regex.IsMatch(password, "['0-9']"))
                sb.Append("Password should be Alphanumberic" + Environment.NewLine);
            if (!Regex.IsMatch(pass, "[<,>,@,!,#,$,%,^,&,*,(,),_,+,\\[,\\],{,},?,:,;,|,',\\,.,/,~,`,-,=]"))
                sb.Append("Password should contain special charcter" + Environment.NewLine);
            return sb.ToString();
        }
    }
}
