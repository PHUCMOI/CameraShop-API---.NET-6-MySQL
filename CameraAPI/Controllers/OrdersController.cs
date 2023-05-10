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
        private readonly Models.CameraAPIdbContext _context;

        private readonly ICameraService _camService;
        private readonly IOrderDetailService _orderDetailService;
        private readonly IOrderService _orderService;
        private readonly IPayPalService _paypalService;

        public OrdersController(Models.CameraAPIdbContext context, ICameraService cameraService, 
            IOrderService orderService, IOrderDetailService orderDetailService, IPayPalService paypalService)
        {
            _context = context;

            _camService = cameraService;
            _orderService = orderService;
            _orderDetailService = orderDetailService;
            _paypalService = paypalService;
        }

        [HttpGet("random")]
        public async Task<ActionResult<OrderRequest>> GetRandomOrder()
        {
            var orderList = await _orderService.GetAllOrder();
            var orderDetailList = await _orderDetailService.GetAllOrderDetail();
            var cameraList = await _camService.GetAllCamera();
            if (!orderList.Any())
            {
                return NotFound();
            }

            var randomIndex = new Random().Next(0, orderList.Count());
            var randomOrder = orderList.ElementAt(randomIndex);

            var totalPrice = orderDetailList.Sum(od =>
            {
                var camera = cameraList.FirstOrDefault(c => c.CameraId == od.CameraId);
                return (camera != null && od.OrderId == randomIndex) ? od.Quantity * camera.Price : 0;
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
                    .Where(p => p.OrderId == randomIndex)
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


            return Ok(orderDetail);
        }

        // GET: api/Orders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
        {
          if (_context.Orders == null)
          {
              return NotFound();
          }
            return await _context.Orders.ToListAsync();
        }

        // GET: api/Orders/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetOrder(int id)
        {
          if (_context.Orders == null)
          {
              return NotFound();
          }
            var order = await _context.Orders.FindAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            return order;
        }

        // PUT: api/Orders/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutOrder(int id, Order order)
        {
            if (id != order.OrderId)
            {
                return BadRequest();
            }

            _context.Entry(order).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpPost("paypal")]
        public async Task<ActionResult<OrderResponse>> PostOrderPayPal(PaymentInformationModel orderRequest)
        {
            var payment = await _paypalService.CreatePaymentUrl(orderRequest);

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

            return Ok(response);
        }

        // POST: api/Orders
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Order>> PostOrder(Order order)
        {
          if (_context.Orders == null)
          {
              return Problem("Entity set 'CameraAPIdbContext.Orders'  is null.");
          }
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetOrder", new { id = order.OrderId }, order);
        }

        // DELETE: api/Orders/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            if (_context.Orders == null)
            {
                return NotFound();
            }
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool OrderExists(int id)
        {
            return (_context.Orders?.Any(e => e.OrderId == id)).GetValueOrDefault();
        }
    }
}
