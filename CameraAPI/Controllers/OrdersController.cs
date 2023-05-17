using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CameraAPI.Models;
using Microsoft.AspNetCore.Authorization;
using CameraAPI.Services.Interfaces;
using CameraService.Services.IRepositoryServices;
using CameraAPI.AppModel;
using CameraService.Services;
using CameraCore.Models;

namespace CameraAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController, Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IPayPalService _paypalService;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(Models.CameraAPIdbContext context, IPayPalService paypalService, ILogger<OrdersController> logger, IOrderService orderService)   
        {
            _paypalService = paypalService;
            _logger = logger;
            _orderService = orderService;
        }

        [HttpGet("random")]
        public async Task<ActionResult<OrderRequest>> GetRandomOrder()
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
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
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

                return BadRequest();
            }
        }

        [HttpPost("paypal")]
        public async Task<ActionResult<OrderResponse>> PostOrderPayPal(PaymentInformationModel orderRequest, decimal? Delivery = null, decimal? Coupon = null)
        {
            if (!Delivery.HasValue)
                Delivery = 0;

            if (!Coupon.HasValue)
                Coupon = 0;

            orderRequest.Price = orderRequest.Price * 10;
            orderRequest.Price /= 100;
            orderRequest.Price = (decimal)(orderRequest.Price + Delivery - Coupon);

            var payment = await _paypalService.CreatePaymentUrl(orderRequest);

            _logger.LogInformation("Price: ", orderRequest.Price.ToString());

            var response = new OrderResponse
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
            _logger.LogCritical("Created Payment.");
            _logger.LogError("error.");
            _logger.LogWarning("Warning.");
            _logger.LogTrace("Trace");

            return Ok(response);
        }

        // POST: api/Orders
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Order>> PostOrder(Order order)
        {
            var orderDetail = await _orderService.Create(order);
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
