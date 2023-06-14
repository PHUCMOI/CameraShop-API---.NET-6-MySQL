using CameraCore.Models;
using CameraService.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace CameraAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmailController : Controller
    {

        private readonly IMailService mailService;
        public EmailController(IMailService mailService)
        {
            this.mailService = mailService;
        }

        [HttpPost("Send")]
        public async Task<IActionResult> Send([FromForm] MailRequest request)
        {
            try
            {
                await mailService.SendEmailAsync(request);
                return Ok();
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }
        }
    }
}
