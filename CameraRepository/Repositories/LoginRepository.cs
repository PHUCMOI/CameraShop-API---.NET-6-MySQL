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

        public UserModel CheckLogin(UserModel _userData)
        {
            var resultLoginCheck = _context.Users
                    .Where(e => e.Username == _userData.Username
                            && e.Password == _userData.Password)
                    .FirstOrDefault();

            if(resultLoginCheck != null)
            {
                var resultUser = new UserModel()
                {
                    Username = resultLoginCheck.Username,
                    Password = resultLoginCheck.Password,
                    AccessToken = null
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
