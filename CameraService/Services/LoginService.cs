using CameraAPI.AppModel;
using CameraCore.IRepository;
using CameraCore.Models;
using CameraService.Services.IServices;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Immutable;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace CameraService.Services
{
    public class LoginService : ILoginService
    {
        private readonly ILoginRepository _loginRepository;
        private readonly IDistributedCache _distributedCache;
        private readonly IConfiguration _configuration;
        public LoginService(ILoginRepository loginRepository, IConfiguration configuration, IDistributedCache distributedCache)
        {
            _loginRepository = loginRepository;
            _configuration = configuration;
            _distributedCache = distributedCache;
        }

        public string Login(UserLogin userObj)
        {
            try
            {
                if (userObj.Username != null)
                {
                    var resultLoginCheck = _loginRepository.CheckLogin(userObj.Username, userObj.Password);


                    /*if (!PasswordHasher.VerifyPassword(userObj.Password, resultLoginCheck.Password))
                    {
                        throw new Exception("Password is not correct!");
                    }*/
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
                        /*string cacheKey = username;
                        byte[] tokenBytes = Encoding.UTF8.GetBytes(AccessToken);

                        var cacheOptions = new DistributedCacheEntryOptions
                        {
                            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
                        };

                        _distributedCache.SetString(cacheKey, AccessToken, cacheOptions);*/

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
                throw new Exception(ex.Message);
            }
        }

        public string CreateRefreshToken()
        {
            var tokenBytes = RandomNumberGenerator.GetBytes(64);
            var refreshToken = Convert.ToBase64String(tokenBytes);

            var tokenInUser = _loginRepository.checkRefreshToken(refreshToken);

            if (tokenInUser)
            {
                return CreateRefreshToken();
            }
            return refreshToken;
        }

        public ClaimsPrincipal GetClaimsPrincipalFromExpiredToken(string token)
        {
            var key = Encoding.ASCII.GetBytes("veryverysceret.....");
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateLifetime = true
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;

            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("This is Invalid Token");
            }

            return principal;
        }
    }
}
