using CameraAPI.AppModel;
using CameraCore.IRepository;
using CameraService.Services.IServices;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CameraService.Services
{
    public class LoginService : ILoginService
    {
        private readonly ILoginRepository _loginRepository;
        private IDistributedCache _distributedCache;
        private readonly IConfiguration _configuration;
        public LoginService(ILoginRepository loginRepository, IConfiguration configuration, IDistributedCache distributedCache)
        {
            _loginRepository = loginRepository;
            _configuration = configuration;
            _distributedCache = distributedCache;
        }

        public string Login(string username, string password)
        {
            try
            {
                if (username != null)
                {
                    var resultLoginCheck = _loginRepository.CheckLogin(username, password);
                    if (resultLoginCheck == null)
                    {
                        throw new Exception();
                    }
                    else
                    {
                        var claims = new[] {
                                    new Claim(JwtRegisteredClaimNames.Sub, _configuration["Jwt:Subject"]),
                                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                                    new Claim(ClaimTypes.Actor, resultLoginCheck.Username),
                                    new Claim(ClaimTypes.NameIdentifier, resultLoginCheck.UserId.ToString()),
                                    new Claim(ClaimTypes.Role, resultLoginCheck.Role),
                                    new Claim(ClaimTypes.Email, resultLoginCheck.Email),
                                    new Claim(ClaimTypes.MobilePhone, resultLoginCheck.PhoneNumber)
                        };


                        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
                        var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                        var token = new JwtSecurityToken(
                            _configuration["Jwt:Issuer"],
                            _configuration["Jwt:Issuer"],
                            claims,
                            expires: DateTime.UtcNow.AddDays(7),
                            signingCredentials: signIn);


                        var AccessToken = new JwtSecurityTokenHandler().WriteToken(token);

                        // Lưu trữ token trong phiên (session)
                        //HttpContext.Session.SetString("Token", _userData.AccessToken);
                        string cacheKey = username;
                        byte[] tokenBytes = Encoding.UTF8.GetBytes(AccessToken);

                        var cacheOptions = new DistributedCacheEntryOptions
                        {
                            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
                        };

                        _distributedCache.SetAsync(cacheKey, tokenBytes, cacheOptions);

                        return AccessToken;
                    }
                }
                else
                {
                    return "Need User Information";
                }
            }
            catch (Exception ex) 
            {
                return ex.Message;
            }
        }
    }
}
