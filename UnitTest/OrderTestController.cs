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
    }
}
