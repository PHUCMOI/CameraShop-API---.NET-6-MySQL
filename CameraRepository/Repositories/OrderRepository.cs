using CameraAPI.AppModel;
using CameraAPI.Models;
using CameraAPI.Repositories;
using CameraCore.IRepository;
using CameraCore.Models;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;

namespace CameraRepository.Repositories
{
    public class OrderRepository : GenericRepository<Order>, IOrderRepository
    {
        private readonly IConfiguration _configuration;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICameraRepository _camRepository;
        private readonly IOrderDetailsRepository _orderDetailsRepository;
        public OrderRepository(CameraAPIdbContext dbContext, IConfiguration configuration, IUnitOfWork unitOfWork, ICameraRepository camRepository, IOrderDetailsRepository orderDetailsRepository) : base(dbContext)
        {
            _configuration = configuration;
            _camRepository = camRepository;
            _orderDetailsRepository = orderDetailsRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<decimal> CreateNewOrder(OrderRequest orderRequest, List<CameraResponse> camera, string userID, decimal orderPrice)
        {
            try
            {
                using(SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("InternShop")))
                {
                    var orderQuery = $@"INSERT INTO [dbo].[Order]
                                                           ([UserId]
                                                           ,[Username]
                                                           ,[Address]
                                                           ,[Payment]
                                                           ,[Status]
                                                           ,[Price]
                                                           ,[Message]
                                                           ,[CreatedBy]
                                                           ,[CreatedDate]
                                                           ,[UpdatedBy]
                                                           ,[UpdatedDate]
                                                           ,[IsDelete])
                                                     VALUES
                                                           ({Convert.ToInt32(userID)}
                                                           ,'{orderRequest.Username}'
                                                           ,'{orderRequest.Address}'
                                                           ,'{orderRequest.Payment}'
                                                           ,'{orderRequest.Status}'
                                                           ,{orderPrice}
                                                           ,'{orderRequest.Message}'
                                                           ,{Convert.ToInt32(userID)}
                                                           ,GETDATE()
                                                           ,{Convert.ToInt32(userID)}
                                                           ,GETDATE()
                                                           ,{0})
                                         SELECT SCOPE_IDENTITY()";
                    await connection.OpenAsync();

                    using (SqlCommand command = new SqlCommand(orderQuery, connection))
                    {
                        var orderId = await command.ExecuteScalarAsync();

                        foreach (var item in camera)
                        {
                            var orderDetailQuery = $@"INSERT INTO [dbo].[OrderDetail]
                                                   ([OrderId]
                                                   ,[CameraId]
                                                   ,[Quantity]
                                                   ,[Status]
                                                   ,[CreatedBy]
                                                   ,[CreatedDate]
                                                   ,[UpdatedBy]
                                                   ,[UpdatedDate]
                                                   ,[IsDelete])
                                            VALUES
                                                   ({orderId}
                                                   ,{item.CameraID}
                                                   ,{item.Quantity}
                                                   ,'payment'
                                                   ,{Convert.ToInt32(userID)}
                                                   ,GETDATE()
                                                   ,{Convert.ToInt32(userID)}
                                                   ,GETDATE()   
                                                   ,{0});";

                            using (SqlCommand orderDetailCommand = new SqlCommand(orderDetailQuery, connection))
                            {
                                await orderDetailCommand.ExecuteNonQueryAsync();
                            }
                        }
                        return (decimal)orderId;
                    }
                }
            }
            catch (Exception ex)
            {
                return 0;
                Console.WriteLine(ex.ToString());   
            }
        }

        public async Task<List<OrderRequestPayPal>> GetOrderList()
        {
            try
            {
                var orderList = new List<Order>();
                using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("InternShop")))
                {
                    var order = @"SELECT o.[OrderId]
                                          ,[UserId]
                                          ,[Username]
                                          ,[Address]
                                          ,[Payment]
                                          ,o.[Status]
                                          ,o.[Price]
                                          ,[Message]
                                      FROM [InternShop].[dbo].[Order] o
                                      WHERE o.Status = 'Prepare'";

                    orderList = (List<Order>)await connection.QueryAsync<Order>(order);
                }

                var orderDetailList = await _orderDetailsRepository.GetAll();
                var cameraList = await _camRepository.GetAll();

                var orderDetails = orderList.Select(order =>
                {
                    var totalPrice = orderDetailList
                        .Where(od => od.OrderId == order.OrderId)
                        .Sum(od =>
                        {
                            var camera = cameraList.FirstOrDefault(c => c.CameraId == od.CameraId);
                            return (camera != null) ? od.Quantity * camera.Price : 0;
                        });

                    var orderDetailsForOrder = orderDetailList
                        .Where(p => p.OrderId == order.OrderId)
                        .Select(p => new OrderDetail1
                        {
                            OrderId = p.OrderId,
                            CameraId = p.CameraId,
                            Quantity = p.Quantity,
                            Status = p.Status,
                            Camera = cameraList
                                .Where(c => c.CameraId == p.CameraId)
                                .Select(c => new Camera1
                                {
                                    Name = c.Name,
                                    CategoryId = c.CategoryId,
                                    Brand = c.Brand,
                                    Description = c.Description,
                                    Price = c.Price,
                                    Img = c.Img
                                })
                                .FirstOrDefault()
                        })
                        .ToList();

                    var orderDetail = new OrderRequestPayPal
                    {
                        OrderId = order.OrderId,
                        UserId = order.UserId,
                        Username = order.Username,
                        Address = order.Address,
                        Payment = order.Payment,
                        Status = order.Status,
                        Price = (decimal)totalPrice,
                        Message = order.Message,
                        OrderDetails = orderDetailsForOrder
                    };

                    return orderDetail;
                }).ToList();

                return orderDetails;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

    }
}
