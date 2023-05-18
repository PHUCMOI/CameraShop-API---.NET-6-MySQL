using CameraAPI.AppModel;
using CameraAPI.Controllers;
using CameraCore.Models;
using CameraService.Services.IRepositoryServices;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CameraService.Services.PayPalService;

namespace UnitTest
{
    public class OrderTestController
    {
        private readonly Mock<IOrderService> _orderServiceMock;
        private readonly Mock<IPayPalService> _paypalServiceMock;
        public OrderTestController()
        {
            _orderServiceMock = new Mock<IOrderService>();
            _paypalServiceMock = new Mock<IPayPalService>();
        }

        [Fact]
        public async Task TestPostOrderPayPal_Success()
        {
            // Arrange
            var orderRequest = new PaymentInformationModel
            {
                OrderId = 1,
                Price = 100 // Giá trị thích hợp cho test case của bạn
                            // Các thuộc tính khác của orderRequest cần thiết cho test case của bạn
            };

            var payment = new PayPalPayment
            {
                url = "https://example.com/paypal-url", // Url giả định trả về từ _paypalService.CreatePaymentUrl
                statusCode = "200",
                errorCode = null,
                Message = "Success"
            };

            _paypalServiceMock.Setup(x => x.CreatePaymentUrl(orderRequest))
                .ReturnsAsync(payment);

            var orderResponse = new OrderResponse
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

            var controller = new OrdersController(_paypalServiceMock.Object, _orderServiceMock.Object);

            // Act
            var result = await controller.PostOrderPayPal(orderRequest);

            // Assert
            // Assert
            var response = Assert.IsAssignableFrom<OrderResponse>(result);

            Assert.Equal(orderResponse.requestID, response.requestID);
            Assert.Equal(orderResponse.orderID, response.orderID);
            Assert.Equal(orderResponse.price, response.price);
            Assert.Equal(orderResponse.responseTime, response.responseTime);
            Assert.Equal(orderResponse.payUrl, response.payUrl);
            Assert.Equal(orderResponse.errorCode, response.errorCode);
            Assert.Equal(orderResponse.statusCode, response.statusCode);
            Assert.Equal(orderResponse.orderStatus, response.orderStatus);

        }

    }
}
