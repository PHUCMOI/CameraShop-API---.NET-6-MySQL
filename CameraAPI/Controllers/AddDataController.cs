using CameraAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.IIS.Core;
using Microsoft.EntityFrameworkCore;

namespace CameraAPI.Controllers
{
    public class AddDataController : Controller
    {
        private readonly CameraAPIdbContext _context;

        [Route("api/[controller]")]
        [HttpPost]
        public IActionResult Index(CameraAPIdbContext info)
        {
            if(ModelState.IsValid)
            {
                using (var db = new CameraAPIdbContext())
                {
                    db.Add(info);
                    db.SaveChanges();
                }
                return Ok("Add ");
            }
            else
            {
                return BadRequest(ModelState);
            } 
        }
    }
}
