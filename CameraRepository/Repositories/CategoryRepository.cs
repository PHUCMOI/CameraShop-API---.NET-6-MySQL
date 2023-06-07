using CameraAPI.Models;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;

namespace CameraAPI.Repositories
{
    public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
    {
        private readonly IConfiguration _configuration;
        public CategoryRepository(CameraAPIdbContext dbContext, IConfiguration configuration) : base(dbContext)
        {
           _configuration = configuration;
        }

        public bool Delete(int categoryId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("InternShop")))
                {
                    var updateQuery = @"UPDATE [InternShop].[dbo].[Category]
                                SET [IsDelete] = 1
                                WHERE [CategoryId] = @categoryId"
                    ;

                    var parameters = new { CategoryId = categoryId };
                    connection.Execute(updateQuery, parameters);
                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<List<Category>> GetCategoryList()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("InternShop")))
                {
                    var query = @"SELECT CategoryId
                                        ,Name
                                        ,CreatedBy
                                        ,CreatedDate
                                        ,UpdatedBy
                                        ,UpdatedDate
                                        ,IsDelete
                                  FROM Category
                                  WHERE isDelete = 0";

                    var categoryList = await connection.QueryAsync<Category>(query);
                    return categoryList.ToList();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
