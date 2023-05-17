using CameraAPI.AppModel;
using CameraAPI.Models;
using CameraAPI.Repositories;
using CameraAPI.Services.Interfaces;
using CameraService.Services.IRepositoryServices;

namespace CameraService.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderDetailService _orderDetailService;
        private readonly ICameraService _camService;
        private readonly IOrderRepository _orderRepository;
        private readonly IUnitOfWork _unitOfWork;
        public OrderService(IOrderRepository orderRepository, IOrderDetailService orderDetailService, ICameraService cameraService, IUnitOfWork unitOfWork)
        {
            _orderRepository = orderRepository;
            _camService = cameraService;
            _orderDetailService = orderDetailService;
            _unitOfWork = unitOfWork;
        }
        public async Task<bool> Create(Order order)
        {
            if (order != null)
            {
                await _unitOfWork.Orders.Create(order);

                //Lưu xuống db 
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

        public async Task<IEnumerable<Order>> GetAllOrder()
        {
            var OrderlList = await _orderRepository.GetAll();
            return OrderlList;
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

        public async Task<OrderRequest> GetRandomOrder()
        {
            var orderList = await _orderRepository.GetAll();
            var orderDetailList = await _orderDetailService.GetAllOrderDetail();
            var cameraList = await _camService.GetAllCamera();

            var randomIndex = new Random().Next(0, orderList.Count());
            var randomOrder = orderList.ElementAt(randomIndex);

            var totalPrice = orderDetailList.Sum(od =>
            {
                var camera = cameraList.FirstOrDefault(c => c.CameraId == od.CameraId);
                return (camera != null && od.OrderId == randomOrder.OrderId) ? od.Quantity * camera.Price : 0;
            });

            var orderDetail = new OrderRequest
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
                                CameraId = c.CameraId,
                                Name = c.Name,
                                CategoryId = c.CategoryId,
                                Brand = c.Brand,
                                Description = c.Description,
                                Price = c.Price,
                                Img = c.Img,
                                Quantity = c.Quantity
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
