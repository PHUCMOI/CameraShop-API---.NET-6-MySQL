using CameraAPI.AppModel;
using CameraAPI.Models;
using CameraAPI.Repositories;
using CameraCore.IRepository;
using CameraCore.Models;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
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
                using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("InternShop")))
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
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<OrderRequestPayPal>> GetOrderList(string? status = null)
        {
            try
            {
                string query = default(string);
                if (!status.IsNullOrEmpty())
                    query += $"AND Status = '{status}'";
                var orderList = new List<Order>();
                using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("InternShop")))
                {
                    var order = $@"SELECT o.[OrderId]
                                          ,[UserId]
                                          ,[Username]
                                          ,[Address]
                                          ,[Payment]
                                          ,o.[Status]
                                          ,o.[Price]
                                          ,[Message]
                                      FROM [InternShop].[dbo].[Order] o
                                      WHERE 1 = 1 {query}";

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
                throw new Exception(ex.Message, ex);
            }
        }

        public async Task<OrderRequestPayPal> GetOrderById(int orderId)
        {
            try
            {
                OrderRequestPayPal orderRequestPayPal = null;

                using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("InternShop")))
                {
                    var orderQuery = $@"SELECT o.[OrderId]
                                      ,[UserId]
                                      ,[Username]
                                      ,[Address]
                                      ,[Payment]
                                      ,o.[Status]
                                      ,o.[Price]
                                      ,[Message]
                                  FROM [InternShop].[dbo].[Order] o
                                  WHERE o.OrderId = @OrderId";

                    var orderParameters = new { OrderId = orderId };
                    var order = await connection.QuerySingleOrDefaultAsync<Order>(orderQuery, orderParameters);

                    if (order != null)
                    {
                        orderRequestPayPal = new OrderRequestPayPal
                        {
                            OrderId = order.OrderId,
                            UserId = order.UserId,
                            Username = order.Username,
                            Address = order.Address,
                            Payment = order.Payment,
                            Status = order.Status,
                            Price = (decimal)order.Price,
                            Message = order.Message
                        };

                        var orderDetailQuery = $@"SELECT [OrderId]
                                               ,[CameraId]
                                               ,[Quantity]
                                               ,[Status]
                                           FROM [InternShop].[dbo].[OrderDetail]
                                           WHERE [OrderId] = @OrderId";

                        var orderDetailParameters = new { OrderId = orderId };
                        var orderDetails = await connection.QueryAsync<OrderDetail>(orderDetailQuery, orderDetailParameters);
                        var orderDetailList = orderDetails.ToList();

                        if (orderDetailList.Any())
                        {
                            orderRequestPayPal.OrderDetails = new List<OrderDetail1>();

                            foreach (var orderDetail in orderDetailList)
                            {
                                var cameraQuery = $@"SELECT [CategoryId]
                                                   ,[Name]
                                                   ,[Brand]
                                                   ,[Description]
                                                   ,[Price]
                                                   ,[Img]
                                               FROM [InternShop].[dbo].[Camera]
                                               WHERE [CameraId] = @CameraId";

                                var cameraParameters = new { CameraId = orderDetail.CameraId };
                                var camera = await connection.QuerySingleOrDefaultAsync<Camera1>(cameraQuery, cameraParameters);

                                var orderDetail1 = new OrderDetail1
                                {
                                    OrderId = orderDetail.OrderId,
                                    CameraId = orderDetail.CameraId,
                                    Quantity = orderDetail.Quantity,
                                    Status = orderDetail.Status,
                                    Camera = camera
                                };

                                orderRequestPayPal.OrderDetails.Add(orderDetail1);
                            }
                        }
                    }
                }

                return orderRequestPayPal;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public bool Delete(int orderId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("InternShop")))
                {
                    var updateOrderQuery = @"UPDATE [InternShop].[dbo].[Order]
                                SET [IsDelete] = 1
                                WHERE [OrderId] = @OrderId";

                    var updateOrderDetailQuery = @"UPDATE [InternShop].[dbo].[OrderDetail]
                                SET [IsDelete] = 1
                                WHERE [OrderId] = @OrderId";

                    var parameters = new { OrderId = orderId };
                    connection.Execute(updateOrderQuery, parameters);
                    connection.Execute(updateOrderDetailQuery, parameters);
                    return true;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void Update(OrderRequest order, string userId, int orderId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("InternShop")))
                {
                    foreach (var orderDetail in order.OrderDetails) 
                    {
                        var updateOrderQuery = $@"UPDATE [dbo].[OrderDetail]
                                   SET [CameraId] = {orderDetail.CameraId}
                                      ,[Quantity] = {orderDetail.Quantity}
                                      ,[Status] = '{orderDetail.Status}'
                                      ,[UpdatedBy] = {Convert.ToInt32(userId)}
                                      ,[UpdatedDate] = GETDATE()
                                    WHERE OrderId = {orderDetail.OrderId} AND CameraId = {orderDetail.CameraId}";
                        var parameters = new { OrderId = orderId };
                        connection.Execute(updateOrderQuery, parameters);
                    }                    
                }    
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void UpdateOrderStatus(int orderId, string status)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("InternShop")))
                {
                    var updateOrderQuery = $@"UPDATE [dbo].[Order]
                                              SET [Status] = '{status}'
                                              WHERE OrderId = {orderId}";
                    connection.Execute(updateOrderQuery);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }
    }
}
