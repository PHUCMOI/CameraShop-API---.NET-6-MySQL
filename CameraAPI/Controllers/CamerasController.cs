using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CameraAPI.Services.Interfaces;
using CameraAPI.Models;
using CameraAPI.AppModel;
using CameraService.Services.IRepositoryServices;
using CameraCore.Models;
using System.Security.Claims;

namespace CameraAPI.Controllers
{
    [Route("api/cameras")]
    [ApiController]
    [Authorize]
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
            var CameraList = await _camService.GetAllCamera();
            if (CameraList == null)
            {
                return NotFound();
            }
            return Ok(CameraList);
        }

        // GET: api/Cameras/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CameraResponseID>> GetCamera(int id)
        {
            var CameraDetail = await _camService.GetIdAsync(id);
            if (CameraDetail != null)
            {
                return Ok(CameraDetail);
            }
            return BadRequest();
        }   

        [HttpGet("linq")]
        public async Task<ActionResult<PaginationCameraResponse>> GetCameraByLINQ(int pageNumber, int? categoryID = null, 
            string? name = null, string? brand = null, decimal? minPrice = null, decimal? maxPrice = null, string? FilterType = null, int? quantity = null)
        {
            var CameraDetail = await _camService.GetCameraByLINQ(pageNumber, categoryID, name, brand, minPrice, maxPrice, FilterType, quantity);
            if (CameraDetail != null)
            {
                return Ok(CameraDetail);
            }
            return BadRequest();
        }

        [HttpGet("raw-query")]
        public async Task<ActionResult<List<PaginationCameraResponse>>> GetCameraByRawQuery(int pageNumber, int? categoryID = null, string? name = null,
            string? brand = null, decimal? minPrice = null, decimal? maxPrice = null, string? FilterType = null, int? quantity = null)
        {
            var CameraDetail = await _camService.GetCameraBySQL(pageNumber, categoryID, name, brand, minPrice, maxPrice, FilterType, quantity);
            if (CameraDetail != null)
            {
                return Ok(CameraDetail);
            }
            return BadRequest();
        }
        
        [HttpGet("stored-procedure")]
        public async Task<ActionResult<List<PaginationCameraResponse>>> GetFromStoredProcedure(int pageNumber, int? categoryID = null, string? name = null,
            string? brand = null, decimal? minPrice = null, decimal? maxPrice = null, string? FilterType = null, int? quantity = null)
        {
            try
            {
                var CameraDetail = await _camService.GetFromStoredProcedure(pageNumber, categoryID, name, brand, minPrice, maxPrice, FilterType, quantity);
                if (CameraDetail != null)
                {
                    return Ok(CameraDetail);
                }
            }
            catch (Exception ex)
            {                
            }
            return BadRequest();
        }

        // PUT: api/Cameras/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCamera(CameraResponse camera, int id)
        {
            try
            {
                var userIdentity = HttpContext.User.Identity as ClaimsIdentity;
                var nameIdentifierValue = userIdentity.Claims.ToList();
                if (camera != null)
                {
                    var CameraDetails = await _camService.Update(camera, nameIdentifierValue[3].Value, id);
                    if (CameraDetails)
                    {
                        return Ok(CameraDetails);
                    }
                }
                return BadRequest();
            }
            catch (Exception ex)            {

                return BadRequest();
            }
        }
        
        // POST: api/Cameras
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Camera>> PostCamera(CameraPostRequest cameraPostRequest)
        {
            var userIdentity = HttpContext.User.Identity as ClaimsIdentity;
            var nameIdentifierValue = userIdentity.Claims.ToList();
            var CameraDetail = await _camService.Create(cameraPostRequest, nameIdentifierValue[3].Value);
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
