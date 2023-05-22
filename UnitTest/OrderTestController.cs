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
                Price = 100
            };

            var payment = new PayPalPayment
            {
                url = "https://example.com/paypal-url",
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
                price = (orderRequest.Price + orderRequest.Price * 10 / 100).ToString(),
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
            var response = Assert.IsAssignableFrom<ActionResult<OrderResponse>>(result);

            Assert.NotNull(response.Value);

            var orderResponseValue = response.Value;

            Assert.Equal(orderResponse.requestID, orderResponseValue.requestID);
            Assert.Equal(orderResponse.orderID, orderResponseValue.orderID);
            Assert.Equal(orderResponse.price, orderResponseValue.price);
            Assert.Equal(orderResponse.payUrl, orderResponseValue.payUrl);
            Assert.Equal(orderResponse.errorCode, orderResponseValue.errorCode);
            Assert.Equal(orderResponse.statusCode, orderResponseValue.statusCode);
            Assert.Equal(orderResponse.orderStatus, orderResponseValue.orderStatus);
        }
    }
}
