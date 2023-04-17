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
using CameraRepository;
using CameraResponse = CameraAPI.AppModel.CameraResponse;
using System.Globalization;
using Nest;
using System.Diagnostics;

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
                        orderby camera.Sold descending
                        select new
                        {
                            Camera = camera,
                            CategoryName = category != null ? category.Name : "N/A",
                            Rank = camera.Sold
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

            var products = cameras.Select(camera => {
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
                                               Description = camera.Description,
                                               BestSeller = "Đã bán " + camera.Sold
                                           };
                                       }
                                  return null;
            })
            .Where(camera => camera != null)
             .ToList();

            return Ok(MapCameraResponse(products));

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
                return Ok(MapCameraResponse(cameras));
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
                return Ok(MapCameraResponse(cameras));
            }
        }        
        #endregion

        #region stored procedure
        [HttpGet("stored-procedure")]
        public async Task<ActionResult<List<PaginationCameraResponse>>> GetFromStoredProcedure(int? categoryID = null, string? name = null,
        string? brand = null, decimal? minPrice = null, decimal? maxPrice = null, int? quantity = null, int? pageSize = null, int? pageNumber = null)
        {
            using (var command = _context.Database.GetDbConnection().CreateCommand())
            {
                if (pageSize.HasValue && pageNumber.HasValue)
                {
                    command.CommandText = "get_camera1";
                    command.Parameters.Add(new SqlParameter("@pagesize", pageSize ?? (object)DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@pagenumber", pageNumber ?? (object)DBNull.Value));
                }
                else
                    command.CommandText = "get_camera";
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

                    return Ok(MapCameraResponse(cameraResponses));
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
        /*private List<PaginationCameraResponse> MapCameraResponse(IEnumerable<CameraResponse> cameras)
        {
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
        }*/
        private IEnumerable<PaginationCameraResponse> MapCameraResponse(IEnumerable<CameraResponse> cameras)
        {
            var cameraList = cameras.ToList();
            var count = cameraList.Count;
            var pageSize = 3;
            var totalPage = (int)Math.Ceiling((decimal)count / pageSize);

            for (var i = 0; i < totalPage; i++)
            {
                yield return new PaginationCameraResponse
                {
                    Camera = cameraList.Skip(i * pageSize).Take(pageSize).ToList(),
                    PageIndex = i + 1,
                    PageSize = pageSize,
                    TotalPage = totalPage
                };
            }
        }

    }
}
