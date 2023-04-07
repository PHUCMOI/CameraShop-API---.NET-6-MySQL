    using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CameraAPI.Services.Interfaces;
using CameraAPI.Models;
using Microsoft.IdentityModel.Tokens;
using CameraAPI.AppModel;
using CameraService.Services.IRepositoryServices;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace CameraAPI.Controllers
{
    [Route("api/cameras")]
    [ApiController]
    [Authorize]
    public class CamerasController : ControllerBase
    {
        private readonly CameraAPIdbContext _context;

        public readonly ICameraService _camService;
        public readonly ICategoryService _categoryService;
        public readonly IOrderDetailService _orderDetailService;

        public CamerasController(ICameraService cameraService, ICategoryService categoryService, CameraAPIdbContext context)
        {
            _context = context;
            _camService = cameraService;
            _categoryService = categoryService;
        }

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

        private async Task<IEnumerable<Camera>> CheckFilterTypeAsync(IEnumerable<Camera> products, 
                                                                string filter, decimal? price = null)
        {
            switch (filter)
            {
                case "lte":
                    products = products.Where(p => p.Price <= price);
                    break;
                case "gte":
                    products = products.Where(p => p.Price >= price);
                    break;
                default:
                    break;
            }
            return products;
        }

        #region USE LINQ 
        [HttpGet("linq")]
        public async Task<ActionResult<IEnumerable<CameraRespone>>> GetCameraByLINQ(int? categoryID = null, string? name = null,
                                                            string? brand = null, decimal? minPrice = null, decimal? maxPrice = null,
                                                            string? FilterType = null, int? quantity = null)
        {
            var cameras = await _camService.GetAllCamera();
            var categories = await _categoryService.GetAllCategory();

            var query = from camera in cameras
                        join category in categories
                            on camera.CategoryId equals category.CategoryId into joinedCategories
                        from category in joinedCategories.DefaultIfEmpty()
                        select new
                        {
                            Camera = camera,
                            CategoryName = category != null ? category.Name : "N/A"
                        };

            if (categoryID.HasValue)
            {
                cameras = cameras.Where(p => p.CategoryId == categoryID.Value);
            }

            if (!string.IsNullOrEmpty(name))
            {
                cameras = cameras.Where(p => p.Name.Contains(name));
            }

            if (!string.IsNullOrEmpty(brand))
            {
                cameras = cameras.Where(p => p.Brand.Contains(brand));
            }

            if (minPrice.HasValue && maxPrice.HasValue)
            {
                cameras = cameras.Where(p => p.Price >= minPrice.Value && p.Price <= maxPrice.Value);
            }
            else if (maxPrice.HasValue || minPrice.HasValue)
            {
                if (!string.IsNullOrEmpty(FilterType) && (maxPrice.HasValue || minPrice.HasValue))
                {
                    decimal? price = maxPrice.HasValue ? maxPrice : minPrice;
                    cameras = await CheckFilterTypeAsync(cameras, FilterType, price);
                }
            }            

            if (quantity.HasValue)
            {
                cameras = cameras.Where(p => p.Quantity == quantity.Value);
            }

            var result = new List<CameraRespone>();
            var cameraIds = cameras.Select(c => c.CameraId).ToList();
            var bestSellers = await CalculateBestSellers(cameraIds);

            foreach (var camera in cameras)
            {
                var category = query.FirstOrDefault(x => x.Camera.CameraId == camera.CameraId);
                if (category != null)
                {
                    result.Add(new CameraRespone
                    {
                        CameraName = camera.Name,
                        Brand = camera.Brand,
                        Price = camera.Price,
                        Img = camera.Img,
                        Quantity = camera.Quantity,
                        CategoryName = category.CategoryName,
                        BestSeller = bestSellers[camera.CameraId],
                        Description = camera.Description
                    });
                }
            }
            return result;
        }

        private async Task<Dictionary<int, string>> CalculateBestSellers(List<int> cameraIds)
        {
            var cameras = await _camService.GetAllCamera();
            var orderDetails = await _context.OrderDetails.ToListAsync();

            var cameraSales = (from od in orderDetails
                               join c in cameras on od.CameraId equals c.CameraId
                               where cameraIds.Contains(c.CameraId)
                               group od by c into g
                               select new
                               {
                                   Camera = g.Key,
                                   QuantitySold = g.Sum(x => x.Quantity)
                               })
                              .OrderByDescending(cs => cs.QuantitySold)
                              .ThenBy(cs => cs.Camera.CameraId)
                              .ToList();

            var bestSellers = new Dictionary<int, string>();

            foreach (var cameraId in cameraIds)
            {
                var camera = cameras.FirstOrDefault(c => c.CameraId == cameraId);

                if (camera == null)
                {
                    bestSellers[cameraId] = $"No camera found with ID {cameraId}.";
                    continue;
                }

                var cameraSale = cameraSales.FirstOrDefault(cs => cs.Camera.CameraId == cameraId);

                if (cameraSale == null)
                {
                    bestSellers[cameraId] = $"No orders found for camera with ID {cameraId}.";
                    continue;
                }

                var result = new StringBuilder();

                var rank = cameraSales.IndexOf(cameraSale) + 1;
                var quantitySold = cameraSale.QuantitySold;

                result.AppendLine($"Top {rank} best-selling cameras with {quantitySold} orders");

                var otherTopSellingCameras = cameraSales
                    .Where(cs => cs.Camera.CameraId != cameraId && cs.Camera.CategoryId == camera.CategoryId)
                    .Take(2);

                foreach (var otherTopSellingCamera in otherTopSellingCameras)
                {
                    var otherRank = cameraSales.IndexOf(otherTopSellingCamera) + 1;
                    var otherQuantitySold = otherTopSellingCamera.QuantitySold;
                    var otherCameraName = otherTopSellingCamera.Camera.Name;

                    //result.AppendLine($"- {otherRank}. {otherCameraName} ({otherQuantitySold} orders)");
                }

                bestSellers[cameraId] = result.ToString();
            }

            return bestSellers;
        }

        #endregion

        [HttpGet("raw-query")]
        public async Task<IActionResult> GetCameraByRawQuery(CameraRespone cameraRespone)
        {

            return Ok();
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
