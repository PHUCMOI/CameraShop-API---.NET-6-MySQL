using CameraAPI.AppModel;
using CameraAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace CameraAPI.Controllers
{
    [ApiController]
    public class LoginController : ControllerBase
    {
        CameraAPIdbContext _context = new CameraAPIdbContext();

        private IConfiguration _configuration;

        public LoginController(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        /*[HttpGet]
        [Route("UserLogin")]
        public IActionResult Login(string Login, string Password)
        {
            UserModel login = new UserModel();
            login.Username = Login;
            login.Password = Password;

            IActionResult response = Unauthorized();
            var user = AuthencationUser(login);
            if (user != null)
            {
                var token = GenerateToken(login);
                login.AccessToken = token;
                response = Ok(new { UserModel = login });
            }
            return response;
        }

        private UserModel AuthencationUser()
        {
            var users = from u in _context.Users
                        select new { u.Username, u.Password };

            UserModel userModel = null;

            foreach (var user in users)
            {
                string username = user.Username;
                string password = user.Password;

            }
            
            return userModel;
        }*/

        /*[HttpPost]
        [Route("Generate")]
        public string GenerateToken(UserModel Login)
        {
            var user = AuthencationUser(Login);
            if(user != null)
            {
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
                var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var claims = new[] {
                         new Claim(JwtRegisteredClaimNames.Sub, Login.Username),
                         new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
             };


                var token = new JwtSecurityToken(
                    _configuration["Jwt:Issuer"],
                    _configuration["Jwt:Issuer"],
                    claims,
                    expires: DateTime.UtcNow.AddMinutes(10),
                    signingCredentials: signIn);


                var Token = new JwtSecurityTokenHandler().WriteToken(token);
                
                return Token;
            }
            return "Failed";

        }*/

        [HttpPost]
        [Route("PostLoginDetails")]
        public IActionResult PostLoginDetails(UserModel _userData)
        {
            if (_userData != null)
            {
                var resultLoginCheck = _context.Users
                    .Where(e => e.Username == _userData.Username 
                            && e.Password == _userData.Password
                            && e.Role == "admin")
                    .FirstOrDefault();
                if (resultLoginCheck == null)
                {
                    return BadRequest("Invalid Credentials");
                }
                else
                {
                    //_userData.UserMessage = "Login Success";

                    var claims = new[] {
                                new Claim(JwtRegisteredClaimNames.Sub, _configuration["Jwt:Subject"]),
                                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                                //new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()),
                                //new Claim("UserId", _userData.ID.ToString()),
                                //new Claim("DisplayName", _userData.FullName),
                                //new Claim("UserName", _userData.FullName),
                                //new Claim("Email", _userData.EmailId)
                            };


                    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
                    var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                    var token = new JwtSecurityToken(
                        _configuration["Jwt:Issuer"],
                        _configuration["Jwt:Issuer"],
                        claims,
                        expires: DateTime.UtcNow.AddDays(7),
                        signingCredentials: signIn);


                    _userData.AccessToken = new JwtSecurityTokenHandler().WriteToken(token);
                 
                    // Lưu trữ token trong phiên (session)
                    //HttpContext.Session.SetString("Token", _userData.AccessToken);

                    return Ok(_userData);
                }
            }
            else
            {
                return BadRequest("No Data Posted");
            }
        }
    }
}

