using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CameraAPI.Services.Interfaces;
using CameraAPI.Models;
using CameraAPI.AppModel;
using CameraService.Services.IRepositoryServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using System.Data;
using Dapper;
using CameraResponse = CameraAPI.AppModel.CameraResponse;
using System.Security.Claims;
using CameraAPI.Repositories;
using System.Net.WebSockets;

namespace CameraAPI.Controllers
{
    [Route("api/cameras")]
    [ApiController]
    [Authorize]
    public class CamerasController : ControllerBase
    {
        private string connectionString = "Server=INTERN-TMPHUC1\\SQLEXPRESS;Database=InternShop;uid=minhphuc;password=minhphuc0159@;Trusted_Connection=True;encrypt=false;";

        private readonly CameraAPIdbContext _context;
        private readonly WarehouseDbContext _warehousecontext; 

        public readonly ICameraService _camService;
        public readonly ICategoryService _categoryService;
        public readonly IOrderDetailService _orderDetailService;

        public readonly IWarehouseCameraService _warehouseCameraService;
        public readonly IWarehouseCategoryService _warehouseCategoryService;

        public CamerasController(ICameraService cameraService, ICategoryService categoryService,
            IWarehouseCameraService warehouseCameraService,
            IWarehouseCategoryService warehouseCategoryService,
            CameraAPIdbContext context, WarehouseDbContext warehouseDbContext)
        {
            _context = context;
            _warehousecontext = warehouseDbContext;

            _camService = cameraService;
            _categoryService = categoryService;

            _warehouseCameraService = warehouseCameraService;
            _warehouseCategoryService = warehouseCategoryService;
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

        private async Task<IEnumerable<CameraQueryResult>> CheckFilterTypeAsync(IEnumerable<CameraQueryResult> products,
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
        public async Task<ActionResult<List<PaginationCameraResponse>>> GetCameraByLINQ(int pageNumber, int? categoryID = null, string? name = null,
        string? brand = null, decimal? minPrice = null, decimal? maxPrice = null,
        string? FilterType = null, int? quantity = null)
        {
            var userIdentity = HttpContext.User.Identity as ClaimsIdentity;

            var cameras = await _camService.GetAllCamera();
            var categories = await _categoryService.GetAllCategory();

            var shopQuery = from camera in cameras
                            join category in categories
                            on camera.CategoryId equals category.CategoryId into joinedCategories
                            from category in joinedCategories.DefaultIfEmpty()
                            orderby camera.Sold descending
                            select new CameraQueryResult
                            {
                                CameraId = camera.CameraId,
                                CameraName = camera.Name,
                                Brand = camera.Brand,
                                Price = (decimal)camera.Price,
                                Sold = camera.Sold,
                                CategoryId = (int)camera.CategoryId,
                                CategoryName = category != null ? category.Name : "N/A",
                                IsWarehouseCamera = false,
                                Quantity = (int)camera.Quantity,
                                Img = camera.Img,
                                Description = camera.Description
                            };

            var result = shopQuery;

            if (userIdentity.HasClaim(c => c.Type == ClaimTypes.Role && c.Value == "admin"))
            {
                var warehouseCamera = await _warehouseCameraService.GetAllCamera();
                var warehouseCategory = await _warehouseCategoryService.GetAllCategory();
                var warehouseQuery = from camera in warehouseCamera
                                     join category in warehouseCategory
                                     on camera.CategoryId equals category.CategoryId into joinedCategories
                                     from category in joinedCategories.DefaultIfEmpty()
                                     orderby camera.Sold descending
                                     select new CameraQueryResult
                                     {
                                         CameraId = camera.CameraId,
                                         CameraName = camera.Name,
                                         Brand = camera.Brand,
                                         Price = (decimal)camera.Price,
                                         Sold = camera.Sold,
                                         CategoryId = (int)camera.CategoryId,
                                         CategoryName = category != null ? category.Name : "N/A",
                                         IsWarehouseCamera = true,
                                         Quantity = (int)camera.Quantity,
                                         Img = camera.Img,
                                         Description = camera.Description
                                     };

                result = result.Union(warehouseQuery);
            }

            if (categoryID.HasValue)
            {
                result = result.Where(p => p.CategoryId == categoryID.Value);
            }

            if (!string.IsNullOrEmpty(name))
            {
                result = result.Where(p => p.CameraName.Contains(name));
            }

            if (!string.IsNullOrEmpty(brand))
            {
                result = result.Where(p => p.Brand.Contains(brand));
            }

            if (minPrice.HasValue && maxPrice.HasValue)
            {
                result = result.Where(p => p.Price >= minPrice.Value && p.Price <= maxPrice.Value);
            }
            else if (maxPrice.HasValue || minPrice.HasValue)
            {
                if (!string.IsNullOrEmpty(FilterType) && (maxPrice.HasValue || minPrice.HasValue))
                {
                    decimal? price = maxPrice.HasValue ? maxPrice : minPrice;
                    result = await CheckFilterTypeAsync(result, FilterType, price);
                }
            }

            if (quantity.HasValue)
            {
                result = result.Where(p => p.Quantity == quantity.Value);
            }

            var products = result.Select(camera =>
            {
                var category = result.FirstOrDefault(x => x.CameraId == camera.CameraId);
                if (category != null)
                {
                    return new CameraResponse
                    {
                        CameraName = camera.CameraName,
                        Brand = camera.Brand,
                        Price = camera.Price,
                        Img = camera.Img,
                        Quantity = camera.Quantity,
                        CategoryName = category.CategoryName,
                        Description = camera.Description,
                        BestSeller = "Đã bán " + camera.Sold
                    };
                }
                return null;
            })
            .Where(camera => camera != null)
             .ToList();

            return Ok(MapCameraResponse(products, pageNumber));
        }
        #endregion

        #region SQL
        [HttpGet("raw-query")]
        public async Task<ActionResult<List<PaginationCameraResponse>>> GetCameraByRawQuery(int pageNumber, int? categoryID = null, string? name = null,
                                                            string? brand = null, decimal? minPrice = null, decimal? maxPrice = null,
                                                            string? FilterType = null, int? quantity = null)
        {
            var userIdentity = HttpContext.User.Identity as ClaimsIdentity;
            // Kiểm tra role của user        
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                // query là câu lệnh trả về gồm danh sách các camera, category name của camera, tổng số lượng camera đó đã được bán và xếp hạng theo số lượng đã được bán
                string query = "";
                if (userIdentity.HasClaim(c => c.Type == ClaimTypes.Role && c.Value == "admin"))
                {
                    query += @"SELECT *, cat.Name AS CategoryName, DENSE_RANK() OVER (ORDER BY c.Sold DESC) AS Rank, c.Sold AS TotalSold
                                FROM (
                                    SELECT * FROM shop.camera
                                    UNION
                                    SELECT * FROM [Warehouse].[warehouse].[Camera]
                                ) AS c
                                JOIN Category cat ON c.CategoryId = cat.CategoryId
                                WHERE 1=1; ";
                }
                if (userIdentity.HasClaim(c => c.Type == ClaimTypes.Role && c.Value == "client"))
                {
                    query += @"SELECT c.*, c.CameraID, c.Name, cat.Name AS CategoryName, TotalSold.TotalSold, 
                               DENSE_RANK() OVER (ORDER BY TotalSold.TotalSold DESC) AS Rank
                                FROM Camera c
                                JOIN (
                                    SELECT od.CameraId, SUM(od.Quantity) AS TotalSold
                                    FROM OrderDetail od
                                    GROUP BY od.CameraId
                                ) AS TotalSold ON c.CameraID = TotalSold.CameraId
                                JOIN Category cat ON c.CategoryId = cat.CategoryId
                                WHERE 1=1 ";
                }
               
                if (categoryID != null)
                {
                    query += " AND c.CategoryID LIKE '%' + @CategoryID + '%'";
                }
                if (name != null)
                {
                    query += " AND c.Name LIKE '%' + @Name + '%'";
                }
                if (brand != null)
                {
                    query += " AND c.Brand LIKE @Brand";
                }
                if (minPrice != null && maxPrice != null)
                {
                    query += " AND c.Price >= @MinPrice AND c.Price <= @MaxPrice";
                }
                else
                {
                    if (FilterType == "lte")
                    {
                        query += " AND c.Price <= @price";
                    }
                    else if (FilterType == "gte")
                    {
                        query += " AND c.Price >= @price";
                    }
                }

                SqlCommand command = new SqlCommand(query, connection);

                if (categoryID != null)
                {
                    command.Parameters.AddWithValue("@CategoryID", categoryID.ToString());
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

                List<CameraResponse> cameras = new List<CameraResponse>();
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
                }

                // Trả về kết quả
                return Ok(MapCameraResponse(cameras, pageNumber));
            }
        }
        #endregion

        #region DAPPER
        [HttpGet("dapper")]
        public async Task<ActionResult<List<PaginationCameraResponse>>> GetCameraByDapper(int pageNumber, int? categoryID = null, string? name = null, string? brand = null, decimal? minPrice = null, decimal? maxPrice = null, string? FilterType = null, int? quantity = null)
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
                    sql += " AND c.CategoryID LIKE '%' + @CategoryID + '%'";
                }
                if (name != null)
                {
                    sql += " AND c.Name LIKE '%' + @Name + '%'";
                }
                if (brand != null)
                {
                    sql += " AND c.Brand LIKE @Brand";
                }
                if (minPrice != null && maxPrice != null)
                {
                    sql += " AND c.Price >= @MinPrice AND Price <= @MaxPrice";
                }
                else
                {
                    if (FilterType == "lte")
                    {
                        sql += " AND c.Price <= @price";
                    }
                    else if (FilterType == "gte")
                    {
                        sql += " AND c.Price >= @price";
                    }
                }

                if(quantity != null)
                {
                    sql += " AND c.Quantity > quantity";
                }

                sql += " ORDER BY TotalSold DESC;";

                decimal? price = maxPrice.HasValue ? maxPrice : minPrice;

                var parameters = new
                {
                    CategoryID = categoryID.ToString(),
                    Name = name,
                    Brand = brand,
                    MinPrice = minPrice,
                    MaxPrice = maxPrice,
                    price = price,
                    quantity = quantity
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
                return Ok(MapCameraResponse(cameras, pageNumber));
            }
        }        
        #endregion

        #region stored procedure
        [HttpGet("stored-procedure")]
        public async Task<ActionResult<List<PaginationCameraResponse>>> GetFromStoredProcedure(int pageNumber, int? categoryID = null, string? name = null,
        string? brand = null, decimal? minPrice = null, decimal? maxPrice = null, int? quantity = null, int? pageSize = null)
        {
            var userIdentity = HttpContext.User.Identity as ClaimsIdentity;

            using (var command = _context.Database.GetDbConnection().CreateCommand())
            {
                if (userIdentity.HasClaim(c => c.Type == ClaimTypes.Role && c.Value == "admin"))
                {
                    command.CommandText = "get_camera_admin";                    
                }
                else if (userIdentity.HasClaim(c => c.Type == ClaimTypes.Role && c.Value == "client"))
                {
                    command.CommandText = "get_camera_client";
                }

                command.CommandType = CommandType.StoredProcedure;

                // Tạo các parameters
                command.Parameters.Add(new SqlParameter("@categoryID", categoryID ?? (object)DBNull.Value));
                command.Parameters.Add(new SqlParameter("@name", name ?? (object)DBNull.Value));
                command.Parameters.Add(new SqlParameter("@brand", brand ?? (object)DBNull.Value));
                command.Parameters.Add(new SqlParameter("@minPrice", minPrice ?? (object)DBNull.Value));
                command.Parameters.Add(new SqlParameter("@maxPrice", maxPrice ?? (object)DBNull.Value));
                command.Parameters.Add(new SqlParameter("@quantity", quantity ?? (object)DBNull.Value));

                await _context.Database.OpenConnectionAsync();
                using (var result = await command.ExecuteReaderAsync())
                {
                    // Map từ Camera sang CameraResponse
                    var cameraResponses = new List<CameraResponse>();
                    while (await result.ReadAsync())
                    {
                        cameraResponses.Add(new CameraResponse
                        {
                            CameraName = result.GetString(result.GetOrdinal("CameraName")),
                            Brand = result.GetString(result.GetOrdinal("Brand")),
                            Price = result.GetDecimal(result.GetOrdinal("Price")),
                            Img = result.GetString(result.GetOrdinal("Img")),
                            Quantity = result.GetInt32(result.GetOrdinal("Quantity")),
                            Description = result.GetString(result.GetOrdinal("Description")),
                            CategoryName = result.GetString(result.GetOrdinal("CategoryName")),
                            BestSeller = result.GetInt32(result.GetOrdinal("Sold")).ToString()
                        });
                    }

                    return Ok(MapCameraResponse(cameraResponses, pageNumber));
                }
            }
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

        // thực hiện mapping để chuyển đổi từ danh sách CameraResponse sang dạng phân trang
        private IEnumerable<PaginationCameraResponse> MapCameraResponse(IEnumerable<CameraResponse> cameras, int pageNumber)
        {
            var cameraList = cameras.ToList();
            var count = cameraList.Count;
            var pageSize = 3;
            var totalPage = (int)Math.Ceiling((decimal)count / pageSize);
            if(pageNumber == 0) pageNumber = 1;
            yield return new PaginationCameraResponse
            {
                Camera = cameraList.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList(),
                PageIndex = pageNumber,
                PageSize = pageSize,
                TotalPage = totalPage
            };
        }
    }
}
