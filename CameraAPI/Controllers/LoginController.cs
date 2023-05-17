using CameraAPI.AppModel;
using CameraAPI.Models;
using CameraService.Services.IRepositoryServices;
using CameraService.Services.IServices;
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
        private readonly ILoginService _loginService;

        public LoginController(ILoginService loginService)
        {
            _loginService = loginService;
        }

        [HttpPost]
        [Route("PostLoginDetails")]
        public async Task<IActionResult> PostLoginDetails(UserModel UserData)
        {
            string accessToken = _loginService.Login(UserData);

            return accessToken == null ? NotFound() : Ok(accessToken);
        }
    }
}

