using CameraAPI.AppModel;
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
        [Route("PostLoginDetails")]
        public async Task<IActionResult> PostLoginDetails(UserModel UserData)
        {
            string accessToken = _loginService.Login(UserData);

            return accessToken == null ? NotFound() : Ok(accessToken);
        }
    }
}

