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
    [Route("api/[controller]")]
    [ApiController, Authorize]
    public class UsersController : ControllerBase
    {
        private readonly Models.CameraAPIdbContext _context;
        private readonly IUserService _userService;
        public UsersController(Models.CameraAPIdbContext context, IUserService userService)
        {
            _context = context;
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
                        var categories = await _userService.Update(user, nameIdentifierValue[3].Value, id);
                        if (categories)
                        {
                            return Ok(categories);
                        }
                    }
                    return BadRequest("failed");
                }
                return BadRequest("user can not use this endpoint");
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }

        // POST: api/Users
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<UserRequest>> PostUser(UserRequest user)
        {
            var userIdentity = HttpContext.User.Identity as ClaimsIdentity;
            var nameIdentifierValue = userIdentity.Claims.ToList();
            if (nameIdentifierValue[4].Value == "admin")
            {
                var result = await _userService.Create(user, nameIdentifierValue[3].Value);
                if (result)
                {
                    return Ok(result);
                }
                return BadRequest("failed");
            }
            return BadRequest("user can not use this endpoint");
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
