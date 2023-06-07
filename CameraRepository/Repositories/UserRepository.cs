using CameraAPI.Models;
using CameraAPI.Repositories;
using Dapper;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CameraRepository.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        private readonly IConfiguration _configuration;
        public UserRepository(CameraAPIdbContext dbContext, IConfiguration configuration) : base(dbContext)
        {
            _configuration = configuration;
        }

        public bool Delete(int userId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("InternShop")))
                {
                    var updateQuery = @"UPDATE [InternShop].[dbo].[User]
                                SET [IsDelete] = 1
                                WHERE [UserId] = @userId"
                    ;

                    var parameters = new { UserId = userId };
                    connection.Execute(updateQuery, parameters);
                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<List<User>> GetUserList()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("InternShop")))
                {
                    var query = @"SELECT UserID
                                        ,Username
                                        ,Password
                                        ,Role
                                        ,Email
                                        ,PhoneNumber
                                        ,Status
                                        ,CreatedBy
                                        ,CreatedDate
                                        ,UpdatedBy
                                        ,UpdatedDate
                                        ,IsDelete
                                 FROM [dbo].[User]
                                 WHERE isDelete = 0";

                    var userList = await connection.QueryAsync<User>(query);
                    return userList.ToList();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
