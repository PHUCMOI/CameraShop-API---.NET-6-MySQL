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
                        if (CheckQuantity((int)item.Quantity, item.CameraID) == true && item.Quantity > 0)
                        {                            
                            i++;
                            orderPrice += item.Price.Value * item.Quantity.Value;               
                        }  
                        else
                        {
                            throw new Exception("Not enough quantity to buy");
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
                else
                {
                    throw new Exception("orderResquest is null");
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw new Exception(ex.Message);
            }
        }

        public Task<bool> DeleteAsync(int orderID)
        {
            if (orderID > 0)
            {
                var Order = _orderRepository.Delete(orderID);
                if (Order)
                {                    
                    var result = _unitOfWork.Save();
                    if (result > 0) return Task.FromResult(true);
                }
            }
            return Task.FromResult(false);
        }

        public async Task<List<PaginationOrderResponse>> GetAllOrder(int pageNumber)
        {
            var orderList = await _orderRepository.GetOrderList();
            return MapOrderResponse(orderList, pageNumber);
        }

        public async Task<OrderRequestPayPal> GetIdAsync(int orderID) 
        {
            if (orderID > 0)
            {
                var order = await _orderRepository.GetOrderById(orderID);
                if (order != null)
                {
                    return order;
                }
            }
            throw new Exception("orderId must greater than 0");
        }        

        public async Task<bool> Update(OrderRequest order, string userId, int orderId) // sửa -> check thanh toán sửa status 
        {
            decimal totalPrice = 0;
            if (order != null)
            {                
                foreach(var item in order.OrderDetails)
                {
                    totalPrice += item.Quantity.Value * item.Camera.Price.Value;
                }    
                var orderDetail = await _orderRepository.GetById(orderId);
                if (orderDetail != null)
                {
                    orderDetail.Address = order.Address;
                    orderDetail.CreatedBy = orderDetail.CreatedBy;
                    orderDetail.CreatedDate = orderDetail.CreatedDate;
                    orderDetail.UpdatedBy = Convert.ToInt32(userId);
                    orderDetail.UpdatedDate = DateTime.Now;
                    orderDetail.Status = order.Status;
                    orderDetail.Message = order.Message;
                    orderDetail.Price = totalPrice;
                    orderDetail.Username = order.Username;
                    orderDetail.UserId = order.UserId;
                    orderDetail.IsDelete = false;
                    orderDetail.Payment = order.Payment;

                    _orderRepository.Update(order, userId, orderId);
                    var result = _unitOfWork.Save();
                    if (result > 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        private List<PaginationOrderResponse> MapOrderResponse(List<OrderRequestPayPal> orders, int pageNumber)
        {
            var orderList = orders.ToList();
            var count = orderList.Count;
            var pageSize = 3;
            var totalPage = (int)Math.Ceiling((decimal)count / pageSize);
            if (pageNumber == 0) pageNumber = 1;

            var paginationResponse = new PaginationOrderResponse
            {
                Orders = orderList.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList(),
                PageIndex = pageNumber,
                PageSize = pageSize,
                TotalPage = totalPage
            };

            return new List<PaginationOrderResponse> { paginationResponse };
        }
    }
}
