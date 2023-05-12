using CameraAPI.AppModel;
using CameraAPI.Models;
using CameraService.Services.IRepositoryServices;
using FiftyOne.Foundation.Mobile.Detection.Caching;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace CameraAPI.Controllers
{
    [ApiController]
    public class LoginController : ControllerBase
    {
        CameraAPIdbContext _context = new CameraAPIdbContext();

        private IConfiguration _configuration;
        private IRedisCacheService _cache;
        private IDistributedCache _distributedCache;

        public LoginController(IConfiguration configuration, IRedisCacheService redisCacheService, IDistributedCache distributedCache)
        {
            _configuration = configuration;
            _cache = redisCacheService;
            _distributedCache = distributedCache;
        }

        [HttpPost]
        [Route("PostLoginDetails")]
        public async Task<IActionResult> PostLoginDetailsAsync(UserModel _userData)
        {
            if (_userData != null)
            {
                var resultLoginCheck = _context.Users
                    .Where(e => e.Username == _userData.Username 
                            && e.Password == _userData.Password)
                    .FirstOrDefault();
                if (resultLoginCheck == null)
                {
                    return BadRequest("Invalid Credentials");
                }
                else
                {
                    var claims = new[] {
                                new Claim(JwtRegisteredClaimNames.Sub, _configuration["Jwt:Subject"]),
                                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), 
                                new Claim(ClaimTypes.Role , resultLoginCheck.Role)
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

                    await _distributedCache.SetAsync(cacheKey, tokenBytes, cacheOptions);
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

