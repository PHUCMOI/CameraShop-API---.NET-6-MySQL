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
using Microsoft.Data.SqlClient;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System.Net.WebSockets;
using System.Drawing.Printing;
using System.Data;
using Dapper;
using Microsoft.AspNetCore.Razor.Language.Extensions;
using NuGet.Packaging.Signing;
using Nest;

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

        private string connectionString = "Server=INTERN-TMPHUC1\\SQLEXPRESS;Database=InternShop;uid=minhphuc;password=minhphuc0159@;Trusted_Connection=True;encrypt=false;";
        
        #region USE LINQ 
        [HttpGet("linq")]
        public async Task<ActionResult<List<PaginationCameraResponse>>> GetCameraByLINQ(int? categoryID = null, string? name = null,
                                                            string? brand = null, decimal? minPrice = null, decimal? maxPrice = null,
                                                            string? FilterType = null, int? quantity = null)
        {
            var cameras = await _camService.GetAllCamera();
            var categories = await _categoryService.GetAllCategory();

            // Trả về bảng gồm danh sách camera tên category tương ứng 
            var query = from camera in cameras
                        join category in categories
                            on camera.CategoryId equals category.CategoryId into joinedCategories
                        from category in joinedCategories.DefaultIfEmpty() // Nếu không có category sẽ trả về N/A
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

            var result = new List<CameraResponse>();

            // Danh sách camera id để truyền vào hàm tính best seller
            var cameraIds = cameras.Select(c => c.CameraId).ToList();
            var bestSellers = await CalculateBestSellersByLINQ(cameraIds);

            // Phân trang với page size = 3
            var cameraPages = new List<PaginationCameraResponse>();
            var pageCount = (int)Math.Ceiling(cameras.Count() / (float)3);

            for (int page = 1; page <= pageCount; page++)
            {
                var products = cameras
                                .Skip((page - 1) * 3)
                                .Take(3)
                                .Select(camera => {
                                    var category = query.FirstOrDefault(x => x.Camera.CameraId == camera.CameraId);
                                    if (category != null)
                                    {
                                        return new CameraResponse
                                        {
                                            CameraName = camera.Name,
                                            Brand = camera.Brand,
                                            Price = camera.Price,
                                            Img = camera.Img,
                                            Quantity = camera.Quantity,
                                            CategoryName = category.CategoryName,
                                            BestSeller = bestSellers[camera.CameraId],
                                            Description = camera.Description
                                        };
                                    }
                                    return null;
                                })
                                .Where(camera => camera != null)
                                .ToList();

                var pageResponse = new PaginationCameraResponse
                {
                    Camera = products,
                    PageIndex = page,
                    PageSize = 3,
                    TotalPage = pageCount,
                };
                cameraPages.Add(pageResponse);
            }
            return cameraPages;
        }

        private async Task<Dictionary<int, string>> CalculateBestSellersByLINQ(List<int> cameraIds)
        {
            var cameras = await _camService.GetAllCamera();
            var orderDetails = await _context.OrderDetails.ToListAsync();

            // Trả về bảng gồm danh sách camera với số lượng được bán và được sắp xếp giảm dần theo số lượng hàng được bán
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

                // Kiểm tra nếu không tìm được camera theo id
                if (camera == null)
                {
                    bestSellers[cameraId] = $"No camera found with ID {cameraId}.";
                    continue;
                }
                                
                var cameraSale = cameraSales.FirstOrDefault(cs => cs.Camera.CameraId == cameraId);

                // Kiểm tra nếu camera được bán hay chưa
                if (cameraSale == null)
                {
                    bestSellers[cameraId] = $"No orders found for camera with ID {cameraId}.";
                    continue;
                }

                var result = new StringBuilder();

                // Sắp xếp vị trí của camera theo số lượng hàng đã bán
                var rank = cameraSales.IndexOf(cameraSale) + 1;
                var quantitySold = cameraSale.QuantitySold;

                result.AppendLine($"Top {rank} best-selling cameras with {quantitySold} orders");

                var otherTopSellingCameras = cameraSales
                    .Where(cs => cs.Camera.CameraId != cameraId && cs.Camera.CategoryId == camera.CategoryId)
                    .Take(2);                

                bestSellers[cameraId] = result.ToString();
            }

            return bestSellers;
        }

        #endregion

        #region SQL
        [HttpGet("raw-query")]
        public async Task<ActionResult<List<PaginationCameraResponse>>> GetCameraByRawQuery(int? categoryID = null, string? name = null,
                                                            string? brand = null, decimal? minPrice = null, decimal? maxPrice = null,
                                                            string? FilterType = null, int? quantity = null)
        {

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                // Trả về gồm danh sách các camera, category name của camera, tổng số lượng camera đó đã được bán và xếp hạng theo số lượng đã được bán
                string query = @"SELECT c.*, c.CameraID, c.Name, cat.Name AS CategoryName, TotalSold.TotalSold, 
                               DENSE_RANK() OVER (ORDER BY TotalSold.TotalSold DESC) AS Rank
                                FROM Camera c
                                JOIN (
                                    SELECT od.CameraId, SUM(od.Quantity) AS TotalSold
                                    FROM OrderDetail od
                                    GROUP BY od.CameraId
                                ) AS TotalSold ON c.CameraID = TotalSold.CameraId
                                JOIN Category cat ON c.CategoryId = cat.CategoryId
                                WHERE 1=1 ";
                if (categoryID != null)
                {
                    query += " AND CategoryID '%' + @CategoryID + '%'";
                }
                if (name != null)
                {
                    query += " AND Name LIKE '%' + @Name + '%'";
                }
                if (brand != null)
                {
                    query += " AND Brand = @Brand";
                }
                if (minPrice != null && maxPrice != null)
                {
                    query += " AND Price >= @MinPrice AND Price <= @MaxPrice";
                }
                else
                {
                    if (FilterType == "lte")
                    {
                        query += " AND Price <= @price";
                    }
                    else if (FilterType == "gte")
                    {
                        query += " AND Price >= @price";
                    }
                }

                SqlCommand command = new SqlCommand(query, connection);

                if (categoryID != null)
                {
                    command.Parameters.AddWithValue("@Category", categoryID);
                }
                if (name != null)
                {
                    command.Parameters.AddWithValue("@Name", name);
                }

                if (brand != null)
                {
                    command.Parameters.AddWithValue("@Brand", brand);
                }

                if (minPrice != null && maxPrice != null)
                {
                    command.Parameters.AddWithValue("@MinPrice", minPrice);
                    command.Parameters.AddWithValue("@MaxPrice", maxPrice);
                }
                else if (minPrice != null)
                {
                    decimal? price = minPrice;
                    command.Parameters.AddWithValue("@MinPrice", minPrice);
                    command.Parameters.AddWithValue("@price", price);
                }
                else if (maxPrice != null)
                {
                    decimal? price = maxPrice;
                    command.Parameters.AddWithValue("@MaxPrice", maxPrice);
                    command.Parameters.AddWithValue("@price", price);
                }

                SqlDataReader reader = await command.ExecuteReaderAsync();

                // Tính toán phân trang
                int pageSize = 3;
                int pageIndex = 1;
                List<CameraResponse> cameras = new List<CameraResponse>();

                var cameraPages = new List<PaginationCameraResponse>();
                int countPages = 0;
                while (reader.Read())
                {
                    // Đọc từng bản ghi từ reader và thêm vào danh sách cameras
                    var camera = new CameraResponse()
                    {
                        CategoryName = reader["CategoryName"].ToString(),
                        CameraName = reader["Name"].ToString(),
                        Brand = reader["Brand"].ToString(),
                        Description = reader["Description"].ToString(),
                        Price = reader["Price"] != DBNull.Value ? (decimal?)reader["Price"] : null,
                        Img = reader["Img"].ToString(),
                        Quantity = reader["Quantity"] != DBNull.Value ? (int?)reader["Quantity"] : null,
                        BestSeller = "Top " + reader["Rank"].ToString() + " seller with " + reader["TotalSold"].ToString() + " orders"
                    };
                    cameras.Add(camera);
                    countPages++;

                    // Nếu đã đọc đủ số lượng camera cho trang hiện tại thì trả về kết quả
                    if (cameras.Count == pageSize)
                    {
                        var pageResponse = new PaginationCameraResponse
                        {
                            Camera = cameras,
                            PageIndex = pageIndex,
                            PageSize = pageSize,
                            TotalPage = (int)Math.Ceiling(countPages / (float)pageSize)
                        };
                        cameraPages.Add(pageResponse);

                        // Chuẩn bị cho trang tiếp theo
                        cameras = new List<CameraResponse>();
                        pageIndex++;
                    }
                }

                int totalPage = (int)Math.Ceiling(countPages / (float)pageSize);

                // Trường hợp còn lại, nếu cameras.Count > 0 tức là có một trang cuối cùng không đủ đủ 3 camera
                if (cameras.Count > 0)
                {
                    var pageResponse = new PaginationCameraResponse
                    {
                        Camera = cameras,
                        PageIndex = pageIndex,
                        PageSize = pageSize,
                        TotalPage = totalPage
                    };
                    cameraPages.Add(pageResponse);
                }

                // Trả về kết quả
                return Ok(cameraPages);
            }
        }
        #endregion

        #region DAPPER
        [HttpGet("dapper")]
        public async Task<ActionResult<List<PaginationCameraResponse>>> GetCameraByDapper(int? categoryID = null, string? name = null,
                                                            string? brand = null, decimal? minPrice = null, decimal? maxPrice = null,
                                                            string? FilterType = null, int? quantity = null)
        {
            using (IDbConnection connection = new SqlConnection(connectionString))
            {
                // Trả về gồm danh sách các camera, category name của camera, tổng số lượng camera đó đã được bán và xếp hạng theo số lượng đã được bán
                var sql = @"SELECT c.*, c.Name AS CameraName, cat.Name AS CategoryName, TotalSold.TotalSold,
                               DENSE_RANK() OVER (ORDER BY TotalSold.TotalSold DESC) AS Rank
                        FROM Camera c
                        JOIN (
                            SELECT od.CameraId, SUM(od.Quantity) AS TotalSold
                            FROM OrderDetail od
                            GROUP BY od.CameraId
                        ) AS TotalSold ON c.CameraID = TotalSold.CameraId
                        JOIN Category cat ON c.CategoryId = cat.CategoryId 
                        WHERE 1=1";                        

                if (categoryID != null)
                {
                    sql += " AND CategoryID '%' + @CategoryID + '%'";
                }
                if (name != null)
                {
                    sql += " AND Name LIKE '%' + @Name + '%'";
                }
                if (brand != null)
                {
                    sql += " AND Brand = @Brand";
                }
                if (minPrice != null && maxPrice != null)
                {
                    sql += " AND Price >= @MinPrice AND Price <= @MaxPrice";
                }
                else
                {
                    if (FilterType == "lte")
                    {
                        sql += " AND Price <= @price";
                    }
                    else if (FilterType == "gte")
                    {
                        sql += " AND Price >= @price";
                    }
                }

                sql += " ORDER BY TotalSold DESC;";

                decimal? price = maxPrice.HasValue ? maxPrice : minPrice;

                var parameters = new
                {
                    CategoryID = categoryID,
                    Name = name,
                    Brand = brand,
                    MinPrice = minPrice,
                    MaxPrice = maxPrice,
                    price = price,
                };
                
                // Dapper để truy xuất dữ liệu và ánh xạ vào CameraResponse
                var cameras = await connection.QueryAsync<CameraResponse, string, decimal, long, CameraResponse>(sql, (camera, categoryName, totalSold, rank) => {
                    camera.CategoryName = categoryName;
                    camera.BestSeller = "Top " + rank.ToString() + " seller";
                    return camera;
                }, 
                parameters, 
                splitOn: "CameraName,CategoryName,TotalSold,Rank"); // Phân tách kêt quả 

                // Trả kết quả bằng cách gọi hàm MapCameraResponse               
                return Ok(MapCameraResponse(cameras));
            }
        }

        private List<PaginationCameraResponse> MapCameraResponse(IEnumerable<CameraResponse> cameras)
        {
            // thực hiện mapping để chuyển đổi từ danh sách CameraResponse sang dạng phân trang
            var cameraList = cameras.ToList();
            var count = cameraList.Count; // Tổng số lượng sản phẩm
            var pageResponses = new List<PaginationCameraResponse>();
            var pageSize = 3;
            var totalPage = (int)Math.Ceiling((decimal)count / pageSize);

            for (var i = 0; i < totalPage; i++)
            {
                var page = new PaginationCameraResponse
                {
                    Camera = cameraList.Skip(i * pageSize).Take(pageSize).ToList(),
                    PageIndex = i + 1,
                    PageSize = pageSize,
                    TotalPage = totalPage
                };
                pageResponses.Add(page);
            }

            return pageResponses;
        }
        #endregion



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
