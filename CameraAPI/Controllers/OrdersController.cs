using Microsoft.AspNetCore.Mvc;
using CameraAPI.Models;
using Microsoft.AspNetCore.Authorization;
using CameraService.Services.IRepositoryServices;
using CameraAPI.AppModel;
using CameraCore.Models;
using System.Security.Claims;

namespace CameraAPI.Controllers
{
    [Route("api/order")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IPayPalService _paypalService;

        public OrdersController(IPayPalService paypalService, IOrderService orderService)   
        {
            _paypalService = paypalService;
            _orderService = orderService;
        }

        // GET: api/Orders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderRequestPayPal>>> GetOrders(int pageNumber)
        {
            var orderList = await _orderService.GetAllOrder(pageNumber);
            if (orderList == null)
            {
                return NotFound();
            }
            return Ok(orderList);
        }

        // GET: api/Orders/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetOrder(int id)
        {
            var orderDetail = await _orderService.GetIdAsync(id);
            if (orderDetail != null)
            {
                return Ok(orderDetail);
            }
            return BadRequest();
        }

        // PUT: api/Orders/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutOrder(OrderRequest order, int id)
        {
            var userIdentity = HttpContext.User.Identity as ClaimsIdentity;
            var nameIdentifierValue = userIdentity.Claims.ToList();
            try
            {
                if (nameIdentifierValue[4].Value == "admin")
                {

                    if (order != null)
                    {
                        var orderResponse = await _orderService.Update(order, nameIdentifierValue[3].Value, id);
                        if (orderResponse)
                        {
                            return Ok(orderResponse);
                        }
                    }
                    return BadRequest("orderResponse is null");
                }
                return BadRequest("user can not use this endpoint");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("paypal")]
        public async Task<ActionResult<OrderResponsePayPal>> PostOrderPayPal(PaymentInformationModel orderRequest, decimal? delivery = null, decimal? coupon = null)
        {
            try
            {                
                if (!delivery.HasValue)
                    delivery = 0;

                if (!coupon.HasValue)
                    coupon = 0;

                if (orderRequest != null) 
                {
                    orderRequest.Price = orderRequest.Price + orderRequest.Price * 10 / 100; // Tax = 10%
                    orderRequest.Price = (decimal)(orderRequest.Price + delivery - coupon);

                    var payment = await _paypalService.CreatePaymentUrl(orderRequest, (decimal)delivery, (decimal)coupon);

                    var response = new OrderResponsePayPal
                    {
                        requestID = orderRequest.OrderId,
                        orderID = orderRequest.OrderId,
                        price = orderRequest.Price.ToString(),
                        responseTime = DateTime.Now,
                        payUrl = payment.url,
                        errorCode = payment.errorCode,
                        statusCode = payment.statusCode,
                        orderStatus = payment.Message
                    };

                    return new ActionResult<OrderResponsePayPal>(response);
                }
                return BadRequest("orderRequest is null");
            }
            catch (Exception ex) 
            {
                return BadRequest(ex.Message);
            }
        }

        // POST: api/Orders
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost("purchaseCamera")]
        public async Task<ActionResult<OrderResponsePayPal>> PostOrder(List<CameraResponse> camera, string address, string payment, string? message = null, decimal? delivery = null, decimal? coupon = null)
        {
            var userIdentity = HttpContext.User.Identity as ClaimsIdentity;
            var nameIdentifierValue = userIdentity.Claims.ToList();
            if (nameIdentifierValue[4].Value == "admin")
            {
                var order = new OrderRequest()
                {
                    UserId = Convert.ToInt16(nameIdentifierValue[3].Value),
                    Username = nameIdentifierValue[2].Value,
                    Address = address,
                    Payment = payment,
                    Status = "Prepare",
                    Price = 0,
                    Message = message
                };
                var orderResponse = await _orderService.Create(order, camera, nameIdentifierValue[3].Value, delivery, coupon);
                if (orderResponse != null)
                {
                    return Ok(orderResponse);
                }
                return BadRequest("orderResponse is null");
            }
            return BadRequest("user can not use this endpoint");
        }

        // DELETE: api/Orders/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var userIdentity = HttpContext.User.Identity as ClaimsIdentity;
            var nameIdentifierValue = userIdentity.Claims.ToList();
            if (nameIdentifierValue[4].Value == "admin")
            {
                var result = await _orderService.DeleteAsync(id);
                if (result)
                {
                    return Ok(result);
                }
                return BadRequest("can not delete");
            }
            return BadRequest("use can not use this endpoint");
        }

        [HttpGet("PaymentSuccess")]
        public async Task<IActionResult> PaymentSuccess([FromQuery] int orderId)
        {
            await _orderService.UpdateOrderStatus(orderId, "Paid");

            return Ok();
        }

        [HttpGet("PaymentFail")]
        public async Task<IActionResult> PaymentFail([FromQuery] int orderId)
        {
            await _orderService.UpdateOrderStatus(orderId, "Prepare");

            return Ok();
        }
    }
}
