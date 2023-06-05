using CameraAPI.AppModel;
using CameraAPI.Models;
using CameraAPI.Repositories;
using CameraAPI.Services.Interfaces;
using CameraCore.Models;
using CameraService.Services.IRepositoryServices;
using CameraService.Services.IServices;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.Extensions.Logging;
using PayPal.v1.Payments;
using static Azure.Core.HttpHeader;

namespace CameraService.Services
{
    public class OrderService : IOrderService
    {
        private readonly IPayPalService _payPalService;
        private readonly ICameraRepository _cameraRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger _logger;
        public OrderService(IOrderRepository orderRepository, IUnitOfWork unitOfWork, ICameraRepository cameraRepository, IPayPalService payPalService)
        {
            _orderRepository = orderRepository;
            _unitOfWork = unitOfWork;
            _cameraRepository = cameraRepository;
            _payPalService = payPalService;
            // _logger = logger;
        }

        public bool CheckQuantity(int quantityBuy, int cameraId)
        {
            var camera = _cameraRepository.GetById(cameraId);

            if(camera == null)
            {
                return false;
            }
            else
            {
                if (quantityBuy > 0)
                {
                    if (quantityBuy < camera.Result.Quantity)
                    {
                        return true;
                    }
                }
                else
                    return false;
            }
            return false;
        }

        public PaymentInformationModel CreatePaymentModel(OrderRequest orderRequest, List<CameraResponse> camera, int orderId, decimal orderPrice)
        {
            List<OrderDetail2> camera2s = new List<OrderDetail2>();

            foreach(var item in camera)
            {
                camera2s.Add(new OrderDetail2()
                {
                    OrderId = orderId,
                    CameraId = item.CameraID,
                    Quantity = item.Quantity,
                    Status = "payment",
                    Camera = new Camera2()
                    {
                        CategoryId = 0,
                        Name = item.CameraName,
                        Brand = item.Brand,
                        Description = item.Description,
                        Price = item.Price,
                        Img = item.Img
                    }
                });
            }    
            var ResponseLinkPaypal = new PaymentInformationModel()
            {
                OrderId = orderId,
                Address = orderRequest.Address,
                Message = orderRequest.Message,
                UserId = orderRequest.UserId,
                Username = orderRequest.Username,
                Payment = orderRequest.Payment,
                Status = orderRequest.Status,
                Price = orderPrice,
                OrderDetails = camera2s
            };

            return ResponseLinkPaypal;
        }

        public async Task<OrderResponsePayPal> Create(OrderRequest orderRequest, List<CameraResponse> camera, string userID, decimal? delivery = null, decimal? coupon = null)
        {
            try
            {
                List<Camera2> camera2s = new List<Camera2>();
                decimal orderId;                
                decimal orderPrice = 0;
                if (orderRequest != null)
                {
                    int i = 0;
                    foreach (var item in camera)
                    {
                        if(CheckQuantity((int)item.Quantity, item.CameraID) == true && item.Quantity > 0)
                        {                            
                            i++;
                            if (item.Price.HasValue && item.Quantity.HasValue)
                            {
                                orderPrice += (item.Price.Value * item.Quantity.Value);
                            }                            
                        }    
                    }

                    if (!delivery.HasValue)
                        delivery = 0;

                    if (!coupon.HasValue)
                        coupon = 0;

                    orderPrice = orderPrice + orderPrice * 10 / 100; // Tax = 10%
                    orderPrice = (decimal)(orderPrice + delivery - coupon);

                    if (i == camera.Count)
                    {
                        orderId = await _orderRepository.CreateNewOrder(orderRequest, camera, userID, orderPrice); // Add to DB

                        // Create paypal link
                        var payment = await _payPalService.CreatePaymentUrl(CreatePaymentModel(orderRequest, camera, Convert.ToInt32(orderId), orderPrice));

                        var response = new OrderResponsePayPal
                        {
                            requestID = Convert.ToInt32(orderId),
                            orderID = Convert.ToInt32(orderId),
                            price = orderPrice.ToString(),
                            responseTime = DateTime.Now,
                            payUrl = payment.url,
                            errorCode = payment.errorCode,
                            statusCode = payment.statusCode,
                            orderStatus = payment.Message
                        };

                        return response;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
               // _logger.LogError("Cannot create order: Failed" + ex.Message);
                Console.WriteLine(ex.ToString());
                return null;
            }
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

        public async Task<IEnumerable<OrderRequestPayPal>> GetAllOrder()
        {
            var OrderList = await _orderRepository.GetOrderList();
            return OrderList;
        }

        public async Task<CameraAPI.Models.Order> GetIdAsync(int OrderID)
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

        public async Task<bool> Update(CameraAPI.Models.Order order)
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
