using CameraAPI.AppModel;
using CameraAPI.Models;
using CameraAPI.Repositories;
using CameraAPI.Services.Interfaces;
using CameraService.Services.IRepositoryServices;
using CameraService.Services.IServices;

namespace CameraService.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderDetailService _orderDetailService;
        private readonly ICameraRepository _camRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAutoMapperService _autoMapperService;
        public OrderService(IOrderRepository orderRepository, IOrderDetailService orderDetailService, ICameraRepository camRepository, IUnitOfWork unitOfWork, IAutoMapperService autoMapperService)
        {
            _orderRepository = orderRepository;
            _camRepository = camRepository;
            _orderDetailService = orderDetailService;
            _unitOfWork = unitOfWork;
            _autoMapperService = autoMapperService;
        }
        public async Task<bool> Create(OrderRequest orderRequest, CameraResponse camera, string UserID, decimal Quantity)
        {
            if (orderRequest != null)
            {
                var order = new Order()
                {
                    UserId = Convert.ToInt32(UserID),
                    Username = orderRequest.Username,
                    Address = orderRequest.Address,
                    Payment = orderRequest.Payment,
                    Status = orderRequest.Status,
                    Price = orderRequest.Price,
                    Message = orderRequest.Message,
                    CreatedBy = Convert.ToInt32(UserID),
                    CreatedDate = DateTime.Now,
                    UpdatedBy = Convert.ToInt32(UserID),
                    UpdatedDate = DateTime.Now,
                    IsDelete = false
                };
                await _orderRepository.Create(order);

                var orderdetail = new OrderDetail()
                {
                    OrderId = order.OrderId,
                    CameraId = camera.CameraID,
                    Quantity = Quantity,
                    Status = "Prepare",
                    CreatedBy = Convert.ToInt32(UserID),
                    CreatedDate = DateTime.Now,
                    UpdatedBy = Convert.ToInt32(UserID),
                    UpdatedDate = DateTime.Now,
                    IsDelete = false
                };
                await _unitOfWork.OrderDetails.Create(orderdetail);

                // Lưu xuống db 
                var result = _unitOfWork.Save();

                if (result > 0)
                    return true;
                else
                    return true;
            }
            return false;
        }

        public async Task<bool> DeleteAsync(int OrderID)
        {
            if (OrderID > 0)
            {
                var Order = await _unitOfWork.Orders.GetById(OrderID);
                if (Order != null)
                {
                    _unitOfWork.Orders.Delete(Order);
                    var result = _unitOfWork.Save();
                    if (result > 0) return true;
                }
            }
            return false;
        }

        public async Task<IEnumerable<OrderResponse>> GetAllOrder()
        {
            var OrderlList = await _orderRepository.GetAll();
            var OrderListResponse = _autoMapperService.MapList<Order, OrderResponse>(OrderlList);
            return OrderListResponse;
        }

        public async Task<Order> GetIdAsync(int OrderID)
        {
            if (OrderID > 0)
            {
                var Order = await _orderRepository.GetById(OrderID);
                if (Order != null)
                {
                    return Order;
                }
            }
            return null;
        }

        public async Task<OrderRequestPayPal> GetRandomOrder()
        {
            var orderList = await _orderRepository.GetAll();
            var orderDetailList = await _orderDetailService.GetAllOrderDetail();
            var cameraList = await _camRepository.GetAll();

            var randomIndex = new Random().Next(0, orderList.Count());
            var randomOrder = orderList.ElementAt(randomIndex);

            var totalPrice = orderDetailList.Sum(od =>
            {
                var camera = cameraList.FirstOrDefault(c => c.CameraId == od.CameraId);
                return (camera != null && od.OrderId == randomOrder.OrderId) ? od.Quantity * camera.Price : 0;
            });

            var orderDetail = new OrderRequestPayPal
            {
                OrderId = randomIndex,
                UserId = randomOrder.UserId,
                Username = randomOrder.Username,
                Address = randomOrder.Address,
                Payment = randomOrder.Payment,
                Status = randomOrder.Status,
                Price = (decimal)totalPrice,
                Message = randomOrder.Message,
                OrderDetails = orderDetailList
                    .Where(p => p.OrderId == randomOrder.OrderId)
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
                    .ToList()
            };
            return orderDetail;
        }

        public async Task<bool> Update(Order order)
        {
            if (order != null)
            {
                var orderDetail = await _unitOfWork.Orders.GetById(order.OrderId);
                if (orderDetail != null)
                {
                    orderDetail.OrderId = order.OrderId;
                    orderDetail.Address = order.Address;
                    orderDetail.CreatedBy = order.CreatedBy;
                    orderDetail.CreatedDate = order.CreatedDate;
                    orderDetail.UpdatedBy = order.UpdatedBy;
                    orderDetail.UpdatedDate = order.UpdatedDate;
                    orderDetail.Status = order.Status;
                    orderDetail.Message = order.Message;
                    orderDetail.Price = order.Price;
                    orderDetail.Username = order.Username;
                    orderDetail.UserId = order.UserId;
                    orderDetail.IsDelete = order.IsDelete;
                    orderDetail.Payment = order.Payment;

                    _unitOfWork.Orders.Update(orderDetail);
                    var result = _unitOfWork.Save();
                    if (result > 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
