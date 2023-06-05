using CameraAPI.AppModel;
using CameraAPI.Models;
using CameraCore.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CameraRepository.Repositories
{
    public class LoginRepository : ILoginRepository
    {
        private readonly CameraAPIdbContext _context;

        public LoginRepository(CameraAPIdbContext context)
        { 
            _context = context;
        }

        public UserModel CheckLogin(string name)
        {
            var resultLoginCheck = _context.Users
                    .Where(e => e.Username == name)
                    .FirstOrDefault();

            if(resultLoginCheck != null)
            {
                var resultUser = new UserModel()
                {
                    Username = resultLoginCheck.Username,
                    UserId = resultLoginCheck.UserId,
                    Role = resultLoginCheck.Role,
                    Email = resultLoginCheck.Email,
                    PhoneNumber = resultLoginCheck.PhoneNumber,
                    AccessToken = ""
                };

                return resultUser;
            }
            else
            {
                return null;
            }    
        }
    }
}
