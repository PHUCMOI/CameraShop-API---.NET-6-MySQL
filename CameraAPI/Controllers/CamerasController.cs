using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CameraAPI.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using CameraAPI.Services.Interfaces;

namespace CameraAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CamerasController : ControllerBase
    {
        public readonly ICameraService _camService;
        public CamerasController(ICameraService cameraService)
        {
            _camService = cameraService;
        }

        /*private readonly CameraAPIdbContext _context;
        public CamerasController(CameraAPIdbContext context)
        {
            _context = context;
        }*/

        // GET: api/Cameras
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Camera>>> GetCameras()
        {
            var CameraList = await _camService.GetAllCamera();
            if (CameraList == null)
            {
                return NotFound();
            }
            return Ok(CameraList);
        }

        // GET: api/Cameras/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Camera>> GetCamera(int id)
        {
            var CameraDetail = await _camService.GetIdAsync(id);
            if (CameraDetail != null)
            {
                return Ok(CameraDetail);
            }
            return BadRequest();    
        }

        // PUT: api/Cameras/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCamera(Camera camera)
        {
            if(camera != null)
            {
                var CameraDetails = await _camService.Update(camera);
                if (CameraDetails)
                {
                    return Ok(CameraDetails);
                }
            }
            return BadRequest();
        }

        // POST: api/Cameras
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Camera>> PostCamera(Camera camera)
        {
            var CameraDetail = await _camService.Create(camera);
            if (CameraDetail)
            {
                return Ok(CameraDetail);
            }
            return BadRequest();
        }

        

        // DELETE: api/Cameras/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCamera(int id)
        {
            var CameraDelete = await _camService.DeleteAsync(id);
            if (CameraDelete)
            {
                return Ok(CameraDelete);
            }   
            return BadRequest();    
        }
        
    }
}
