using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CameraAPI.Models;
using Microsoft.AspNetCore.JsonPatch;

namespace CameraAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CamerasController : ControllerBase
    {
        private readonly CameraAPIdbContext _context;

        public CamerasController(CameraAPIdbContext context)
        {
            _context = context;
        }

        // GET: api/Cameras
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Camera>>> GetCameras()
        {
          if (_context.Cameras == null)
          {
              return NotFound();
          }
            return await _context.Cameras.ToListAsync();
        }

        // GET: api/Cameras/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Camera>> GetCamera(int id)
        {
          if (_context.Cameras == null)
          {
              return NotFound();
          }
            var camera = await _context.Cameras.FindAsync(id);

            if (camera == null)
            {
                return NotFound();
            }

            return camera;
        }

        // PUT: api/Cameras/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCamera(int id, Camera camera)
        {
            if (id != camera.CameraId)
            {
                return BadRequest();
            }

            _context.Entry(camera).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CameraExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpPatch("{id}")]
        public IActionResult UpdateCamera(int id, [FromBody] JsonPatchDocument<Camera> patchDoc)
        {
            var camera = _context.Cameras.Find(id);
            if (camera == null)
            {
                return NotFound();
            }

            patchDoc.ApplyTo(camera);
            _context.Update(camera);

            return NoContent();
        }

        // POST: api/Cameras
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Camera>> PostCamera(Camera camera)
        {
          if (_context.Cameras == null)
          {
              return Problem("Entity set 'CameraAPIdbContext.Cameras'  is null.");
          }
            _context.Cameras.Add(camera);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCamera", new { id = camera.CameraId }, camera);
        }

        

        // DELETE: api/Cameras/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCamera(int id)
        {
            if (_context.Cameras == null)
            {
                return NotFound();
            }
            var camera = await _context.Cameras.FindAsync(id);
            if (camera == null)
            {
                return NotFound();
            }
            camera.IsDelete = true;

            //_context.Cameras.Remove(camera);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CameraExists(int id)
        {
            return (_context.Cameras?.Any(e => e.CameraId == id)).GetValueOrDefault();
        }
    }
}
