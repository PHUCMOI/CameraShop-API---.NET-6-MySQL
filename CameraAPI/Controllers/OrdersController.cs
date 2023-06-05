using Microsoft.AspNetCore.Mvc;
using CameraAPI.Models;
using Microsoft.AspNetCore.Authorization;
using CameraService.Services.IRepositoryServices;
using CameraAPI.AppModel;
using CameraCore.Models;
using System.Security.Claims;

namespace CameraAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController, Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IPayPalService _paypalService;

        public OrdersController(IPayPalService paypalService, IOrderService orderService)   
        {
            _paypalService = paypalService;
            _orderService = orderService;
        }

        [HttpGet("random")]
        public async Task<ActionResult<OrderRequestPayPal>> GetRandomOrder()
        {
            var order = await _orderService.GetRandomOrder();
            if (order == null)
            {
                return NotFound();
            }
            return Ok(order);
        }

        // GET: api/Orders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderResponse>>> GetOrders()
        {
            var orderList = await _orderService.GetAllOrder();
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
            var OrderDetail = await _orderService.GetIdAsync(id);
            if (OrderDetail != null)
            {
                return Ok(OrderDetail);
            }
            return BadRequest();
        }

        // PUT: api/Orders/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutOrder(Order order)
        {
            try
            {
                if (order != null)
                {
                    var CameraDetails = await _orderService.Update(order);
                    if (CameraDetails)
                    {
                        return Ok(CameraDetails);
                    }
                }
                return BadRequest();
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            }
        }

        [HttpPost("paypal")]
        public async Task<ActionResult<OrderResponsePayPal>> PostOrderPayPal(PaymentInformationModel orderRequest, decimal? Delivery = null, decimal? Coupon = null)
        {
            try
            {                
                if (!Delivery.HasValue)
                    Delivery = 0;

                if (!Coupon.HasValue)
                    Coupon = 0;

                if (orderRequest != null) 
                {
                    orderRequest.Price = orderRequest.Price + orderRequest.Price * 10 / 100;
                    orderRequest.Price = (decimal)(orderRequest.Price + Delivery - Coupon);

                    var payment = await _paypalService.CreatePaymentUrl(orderRequest);

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
                return BadRequest();
            }
            catch (Exception ex) 
            {
                return BadRequest(ex.Message);
            }
        }

        // POST: api/Orders
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<OrderRequestPayPal>> PostOrder(CameraResponse camera, string Address, string Payment, decimal Quantity, string? Message = null)
        {
            var userIdentity = HttpContext.User.Identity as ClaimsIdentity;
            var nameIdentifierValue = userIdentity.Claims.ToList();
            var order = new OrderRequest()
            {
                UserId = Convert.ToInt16(nameIdentifierValue[3].Value),
                Username = nameIdentifierValue[2].Value,
                Address = Address,
                Payment = Payment,
                Status = "Prepare",
                Price = 0,
                Message = Message
            };
            var orderDetail = await _orderService.Create(order, camera, nameIdentifierValue[3].Value, Quantity);
            if (orderDetail)
            {
                return Ok(orderDetail);
            }
            return BadRequest();
        }

        // DELETE: api/Orders/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var orderDetail = await _orderService.DeleteAsync(id);
            if (orderDetail)
            {
                return Ok(orderDetail);
            }
            return BadRequest();
        }
    }
}
