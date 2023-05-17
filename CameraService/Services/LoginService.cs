using CameraAPI.AppModel;
using CameraCore.IRepository;
using CameraService.Services.IServices;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

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

        public string Login(UserModel _userData)
        {
            if (_userData != null)
            {
                var resultLoginCheck = _loginRepository.CheckLogin(_userData);
                if (resultLoginCheck == null)
                {
                    return null;
                }
                else
                {
                    var claims = new[] {
                                new Claim(JwtRegisteredClaimNames.Sub, _configuration["Jwt:Subject"]),
                                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
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
                    string cacheKey = "user:token:" + _userData.Username;
                    byte[] tokenBytes = Encoding.UTF8.GetBytes(_userData.AccessToken);

                    var cacheOptions = new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
                    };

                    _distributedCache.SetAsync(cacheKey, tokenBytes, cacheOptions);


                    return _userData.AccessToken;
                }
            }
            else
            {
                return null;
            }
        }
    }
}
