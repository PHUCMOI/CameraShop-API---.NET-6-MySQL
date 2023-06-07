using CameraAPI.AppModel;
using CameraAPI.Models;
using CameraCore.Models;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PayPal.v1.Sync;
using System.Data;
using System.Data.SqlClient;
using System.Drawing.Drawing2D;
using System.Net.Http;
using System.Security.Claims;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace CameraAPI.Repositories
{
    public class CamerasRepository : GenericRepository<Camera>, ICameraRepository
    {
        private readonly IConfiguration _configuration;

        public CamerasRepository(CameraAPIdbContext dbContext, IConfiguration configuration) : base(dbContext)
        {
            _configuration = configuration;
        }

        private string CalculateSQLString(string query, int? categoryID = null, string? name = null, string? brand = null, decimal? minPrice = null, decimal? maxPrice = null, string? FilterType = null, int? quantity = null)
        {
            if (categoryID != null)
            {
                query += " AND c.CategoryID = @CategoryID ";
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
                    query += " AND c.Price <= @Price";
                }
                else if (FilterType == "gte")
                {
                    query += " AND c.Price >= @Price";
                }
            }
            return query;
        }

        public async Task<List<CameraResponse>> GetBySQL(int pageNumber, int? categoryID = null, string? name = null, string? brand = null, decimal? minPrice = null, decimal? maxPrice = null, string? FilterType = null, int? quantity = null)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("InternShop")))
                {
                    await connection.OpenAsync();

                    // câu lệnh trả về gồm danh sách các camera, category name của camera,
                    // và xếp hạng theo số lượng đã được bán
                    string query = @"SELECT *, c.Name AS CameraName, cat.Name AS CategoryName,    
                                DENSE_RANK() OVER (ORDER BY c.Sold DESC) AS Rank
                                FROM (
                                    SELECT * FROM dbo.camera
                                    UNION
                                    SELECT * FROM [Warehouse].[warehouse].[Camera]
                                ) AS c
                                JOIN Category cat ON c.CategoryId = cat.CategoryId
                                WHERE c.isDelete = 0 ";

                    query = CalculateSQLString(query, categoryID, name, brand, minPrice, maxPrice, FilterType, quantity);

                    decimal? price = maxPrice.HasValue ? maxPrice : minPrice;

                    var parameters = new
                    {
                        CategoryID = categoryID,
                        Name = name,
                        Brand = brand,
                        MinPrice = minPrice,
                        MaxPrice = maxPrice,
                        Price = price,
                        Quantity = quantity
                    };

                    // Dapper để truy xuất dữ liệu và ánh xạ vào CameraResponse
                    var cameras = await connection.QueryAsync<CameraResponse, string, long, CameraResponse>(
                        query,
                        (camera, categoryName, rank) =>
                        {
                            var cameraResponse = new CameraResponse
                            {
                                CameraID = camera.CameraID,
                                CameraName = camera.CameraName,
                                Brand = camera.Brand,
                                Price = camera.Price,
                                Img = camera.Img,
                                Quantity = camera.Quantity,
                                CategoryName = categoryName,
                                Description = camera.Description,
                                BestSeller = "Top " + rank.ToString() + " seller"
                            };
                            return cameraResponse;
                        },
                        parameters,
                        splitOn: "CategoryName,Rank"
                    );

                    return cameras.ToList();
                }                
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<List<CameraResponse>> GetByStoredProcedure(int pageNumber, int? categoryID = null, string? name = null, string? brand = null, decimal? minPrice = null, decimal? maxPrice = null, string? FilterType = null, int? quantity = null)
        {
            try
            {
                using (var command = _context.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = "get_camera_admin";
                    command.CommandType = CommandType.StoredProcedure;

                    // Tạo các parameters
                    command.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@categoryID", categoryID ?? (object)DBNull.Value));
                    command.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@name", name ?? (object)DBNull.Value));
                    command.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@brand", brand ?? (object)DBNull.Value));
                    command.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@minPrice", minPrice ?? (object)DBNull.Value));
                    command.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@maxPrice", maxPrice ?? (object)DBNull.Value));
                    command.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@quantity", quantity ?? (object)DBNull.Value));
                    command.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@Filter", FilterType ?? (object)DBNull.Value));

                    await _context.Database.OpenConnectionAsync();
                    using (var result = await command.ExecuteReaderAsync())
                    {
                        // Map từ Camera sang CameraResponse
                        var cameraResponses = new List<CameraResponse>();
                        while (await result.ReadAsync())
                        {
                            cameraResponses.Add(new CameraResponse
                            {
                                CameraID = result.GetInt32(result.GetOrdinal("CameraID")),
                                CameraName = result.GetString(result.GetOrdinal("CameraName")),
                                Brand = result.GetString(result.GetOrdinal("Brand")),
                                Price = result.GetDecimal(result.GetOrdinal("Price")),
                                Img = result.GetString(result.GetOrdinal("Img")),
                                Quantity = result.GetInt32(result.GetOrdinal("Quantity")),
                                Description = result.GetString(result.GetOrdinal("Description")),
                                CategoryName = result.GetString(result.GetOrdinal("CategoryName")),
                                BestSeller = "Top " + result.GetInt64(result.GetOrdinal("SoldRank")).ToString() + " Seller"
                            });
                        }

                        return cameraResponses;
                    }
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public bool Delete(int cameraId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("InternShop")))
                {
                    var updateQuery = @"UPDATE [InternShop].[dbo].[Camera]
                                SET [IsDelete] = 1
                                WHERE [CameraId] = @cameraId";

                    var parameters = new { CameraId = cameraId };
                    connection.Execute(updateQuery, parameters);
                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<List<Camera>> GetCameraList()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("InternShop")))
                {
                    var query = @"SELECT
                                    c.CameraID,
                                    c.Name,
                                    c.Brand,
                                    c.Price,
                                    c.Img,
                                    c.Quantity,
                                    c.CategoryId,
                                    c.Description,
                                    c.Sold,
                                    c.CreatedBy,
                                    c.CreatedDate,
                                    c.UpdatedBy,
                                    c.UpdatedDate,
                                    c.IsDelete
                                FROM
                                    Camera c
                                WHERE
                                    c.isDelete = 0
                            ";

                    var cameraList = await connection.QueryAsync<Camera>(query);
                    return cameraList.ToList();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
        
            
        
    

