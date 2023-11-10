using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CameraAPI.Services.Interfaces;
using CameraAPI.Models;
using CameraAPI.AppModel;
using CameraService.Services.IRepositoryServices;
using CameraCore.Models;
using System.Security.Claims;
using Nest;

namespace CameraAPI.Controllers
{
    [Route("api/cameras")]
    [ApiController]
    //[Authorize]
    public class CamerasController : ControllerBase
    {
        public readonly ICameraService _camService;
        public readonly ICategoryService _categoryService;

        public readonly IWarehouseCameraService _warehouseCameraService;
        public readonly IWarehouseCategoryService _warehouseCategoryService;

        public CamerasController(ICameraService cameraService, ICategoryService categoryService,
            IWarehouseCameraService warehouseCameraService,
            IWarehouseCategoryService warehouseCategoryService)
        {            
            _camService = cameraService;
            _categoryService = categoryService;

            _warehouseCameraService = warehouseCameraService;
            _warehouseCategoryService = warehouseCategoryService;
        }

        // GET: api/Cameras
        [HttpGet]
        public async Task<ActionResult<List<CameraResponse>>> GetCameras()
        {
            var cameraList = await _camService.GetAllCamera();
            if (cameraList == null)
            {
                return NotFound();
            }
            return Ok(cameraList);
        }

        // GET: api/Cameras/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CameraResponseID>> GetCamera(int id)
        {
            var cameraDetail = await _camService.GetIdAsync(id);
            if (cameraDetail != null)
            {
                return Ok(cameraDetail);
            }
            return BadRequest("This camera has been deleted");
        }   

        [HttpGet("linq")]
        public async Task<ActionResult<PaginationCameraResponse>> GetCameraByLINQ(int pageNumber, int? categoryID = null, 
            string? name = null, string? brand = null, decimal? minPrice = null, decimal? maxPrice = null, string? filterType = null)
        {
            var cameraDetail = await _camService.GetCameraByLINQ(pageNumber, categoryID, name, brand, minPrice, maxPrice, filterType);
            if (cameraDetail != null)
            {
                return Ok(cameraDetail);
            }
            return BadRequest();
        }

        [HttpGet("raw-query")]
        public async Task<ActionResult<List<PaginationCameraResponse>>> GetCameraByRawQuery(int pageNumber, int? categoryID = null, string? name = null,
            string? brand = null, decimal? minPrice = null, decimal? maxPrice = null, string? filterType = null)
        {
            var cameraDetail = await _camService.GetCameraBySQL(pageNumber, categoryID, name, brand, minPrice, maxPrice, filterType);
            if (cameraDetail != null)
            {
                return Ok(cameraDetail);
            }
            return BadRequest();
        }
        
        [HttpGet("stored-procedure")]
        public async Task<ActionResult<List<PaginationCameraResponse>>> GetFromStoredProcedure(int pageNumber, int? categoryID = null, string? name = null,
            string? brand = null, decimal? minPrice = null, decimal? maxPrice = null, string? filterType = null)
        {
            try
            {
                var cameraDetail = await _camService.GetFromStoredProcedure(pageNumber, categoryID, name, brand, minPrice, maxPrice, filterType);
                if (cameraDetail != null)
                {
                    return Ok(cameraDetail);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return BadRequest();
        }

        // PUT: api/Cameras/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCamera(CameraPostRequest camera, int id)
        {
            try
            {
                var userIdentity = HttpContext.User.Identity as ClaimsIdentity;
                var nameIdentifierValue = userIdentity.Claims.ToList();
                if (nameIdentifierValue[4].Value == "admin")
                {
                    if (camera != null)
                    {
                        var cameraDetails = await _camService.Update(camera, nameIdentifierValue[3].Value, id);
                        if (cameraDetails)
                        {
                            return Ok(cameraDetails);
                        }
                    }
                    return BadRequest("camera is null");
                }
                return BadRequest("This user can not use this endpoint");
            }
            catch (Exception ex) {

                return BadRequest(ex.Message);
            }
        }
        
        // POST: api/Cameras
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<bool>> PostCamera(CameraPostRequest cameraPostRequest)
        {
            var userIdentity = HttpContext.User.Identity as ClaimsIdentity;
            var nameIdentifierValue = userIdentity.Claims.ToList();
            if (nameIdentifierValue[4].Value == "admin")
            {
                if (cameraPostRequest != null)
                {
                    var cameraDetails = await _camService.Create(cameraPostRequest, nameIdentifierValue[3].Value);
                    if (cameraDetails)
                    {
                        return Ok(cameraDetails);
                    }
                }
                    return BadRequest("camera is null");
            }
            return BadRequest("This user can not use this endpoint");
        }

        // DELETE: api/Cameras/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCamera(int id)
        {
            var userIdentity = HttpContext.User.Identity as ClaimsIdentity;
            var nameIdentifierValue = userIdentity.Claims.ToList();
            if (nameIdentifierValue[4].Value == "admin")
            {
                var cameraDelete = await _camService.DeleteAsync(id);
                if (cameraDelete)
                {
                    return Ok(cameraDelete);
                }
                return BadRequest();
            }
            return BadRequest("This user can not use this endpoint");
        }
    }
}
