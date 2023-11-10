using CameraAPI.AppModel;
using CameraAPI.Models;
using CameraCore.Models;
using CameraService.Services.IServices;
using Microsoft.AspNetCore.Mvc;

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
        [Route("login")]
        public async Task<IActionResult> PostLoginDetails([FromBody] UserLogin userObj)
        {
            try
            {
                string accessToken = _loginService.Login(userObj);

                return Ok(new TokenApiDTO()
                {
                    AccessToken = accessToken,
                    RefreshToken = ""
                });                
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}

