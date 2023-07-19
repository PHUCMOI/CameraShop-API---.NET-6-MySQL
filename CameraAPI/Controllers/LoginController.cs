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
        public Task<IActionResult> PostLoginDetails([FromBody] UserLogin userObj)
        {
            try
            {
                string accessToken = _loginService.Login(userObj);

                return Task.FromResult<IActionResult>(accessToken == null ? NotFound() : Ok(accessToken));
            }
            catch (Exception ex)
            {
                return Task.FromResult<IActionResult>(BadRequest(ex.Message));
            }
        }
    }
}

