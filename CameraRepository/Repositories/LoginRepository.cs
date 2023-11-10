using CameraAPI.AppModel;
using CameraAPI.Models;
using CameraCore.IRepository;
using CameraCore.Models;
using Dapper;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CameraRepository.Repositories
{
    public class LoginRepository : ILoginRepository
    {
        private readonly CameraAPIdbContext _context;
        private readonly IConfiguration _configuration;

        public LoginRepository(CameraAPIdbContext context, IConfiguration configuration)
        { 
            _context = context;
            _configuration = configuration;
        }

        public UserModel CheckLogin(string name, string password)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("InternShop")))
                {
                    var query = @"SELECT Username, Password, UserId, Role, Email, PhoneNumber 
                                    FROM [dbo].[User]
                                    WHERE Username = @name AND Password = @password"
                    ;

                    var parameters = new { name = name, password = password };

                    var user = connection.QueryFirstOrDefault<User>(query, parameters);

                    if (user != null)
                    {
                        var resultUser = new UserModel()
                        {
                            Password = user.Password,
                            Username = user.Username,
                            UserId = user.UserId,
                            Role = user.Role,
                            Email = user.Email,
                            PhoneNumber = user.PhoneNumber,
                            AccessToken = ""
                        };

                        return resultUser;
                    }
                    else
                    {
                        throw new Exception("username or password is not correct");
                    }
                }                
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }                
        }

        public Boolean checkRefreshToken(string refreshtoken)
        {
            var tokenInUser = _context.Users.Any(a => a.Username == refreshtoken);

            if(tokenInUser)
            {
                return true;
            }
            return false;
        }
    }
}
