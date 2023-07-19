using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CameraAPI.Models;
using Microsoft.AspNetCore.Authorization;
using CameraAPI.Repositories;
using CameraService.Services.IServices;
using CameraCore.Models;
using System.Net.WebSockets;
using System.Security.Claims;

namespace CameraAPI.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserResponse>>> GetUsers()
        {
            var userList = await _userService.GetAllUser();
            if(userList != null)
            {
                return Ok(userList);
            }
            return BadRequest();
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserResponse>> GetUser(int id)
        {
            var user = await _userService.GetIdAsync(id);
            if(user != null)
            {
                return Ok(user);
            }
            return BadRequest("User has been deleted");
        }

        // PUT: api/Users/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, UserRequest user)
        {
            try
            {
                var userIdentity = HttpContext.User.Identity as ClaimsIdentity;
                var nameIdentifierValue = userIdentity.Claims.ToList();
                if (nameIdentifierValue[4].Value == "admin")
                {

                    if (user != null)
                    {
                        var userDetail = await _userService.Update(user, nameIdentifierValue[3].Value, id);
                        if (userDetail)
                        {
                            return Ok(userDetail);
                        }
                    }
                    return BadRequest("failed");
                }
                return BadRequest("user can not use this endpoint");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // POST: api/Users
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost("register")]
        public async Task<IActionResult> PostUser([FromBody] UserRequest user)
        {
            /*var userIdentity = HttpContext.User.Identity as ClaimsIdentity;
            var nameIdentifierValue = userIdentity.Claims.ToList();*/
            var result = await _userService.Create(user);
            if (result)
            {
                return Ok(result);
            }
            return BadRequest("failed");
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var userIdentity = HttpContext.User.Identity as ClaimsIdentity;
            var nameIdentifierValue = userIdentity.Claims.ToList();
            if (nameIdentifierValue[4].Value == "admin")
            {
                var result = await _userService.DeleteAsync(id);
                if (result)
                {
                    return Ok(result);
                }
                return BadRequest("failed");
            }
            return BadRequest("user can not use this endpoint");
        }
    }
}
