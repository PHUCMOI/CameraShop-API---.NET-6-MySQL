using CameraAPI.AppModel;
using CameraAPI.Controllers;
using CameraAPI.Models;
using CameraAPI.Repositories;
using CameraAPI.Services;
using CameraAPI.Services.Interfaces;
using CameraService.Services.IRepositoryServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using Moq;
using Nest;
using PayPal.v1.Orders;
using System.Drawing.Printing;
using System.Security.Claims;
using Xunit.Sdk;

namespace UnitTest
{
    public class CameraTestController
    {
        private readonly Mock<ICameraService> _cameraServiceMock;
        private readonly Mock<ICategoryService> _categoryServiceMock;
        private readonly Mock<IWarehouseCameraService> _warehouseCameraServiceMock;
        private readonly Mock<IWarehouseCategoryService> _warehouseCategoryServiceMock;

        public CameraTestController()
        {
            _cameraServiceMock = new Mock<ICameraService>();
            _categoryServiceMock = new Mock<ICategoryService>();
            _warehouseCameraServiceMock = new Mock<IWarehouseCameraService>();
            _warehouseCategoryServiceMock = new Mock<IWarehouseCategoryService>();
        }

        [Fact]
        public async Task TestGetAllCamera()
        {
            var cameraList = GetTestCameras();
            _cameraServiceMock.Setup(x => x.GetAllCamera()).Returns(cameraList);
            var cameraController = new CamerasController(
                    _cameraServiceMock.Object,
                    _categoryServiceMock.Object,
                    _warehouseCameraServiceMock.Object,
                    _warehouseCategoryServiceMock.Object
                );
            var cameraResult = await cameraController.GetCameras();

            Assert.NotNull(cameraResult);
        }

        [Fact]
        public async Task TestGetCameraByLINQ()
        {
            // Arrange  
            var cameraList = await GetTestCameras();
            var camera = cameraList[1];
            _cameraServiceMock.Setup(x => x.GetAllCamera()).ReturnsAsync(cameraList);

            var cameraController = new CamerasController(
                 _cameraServiceMock.Object,
                 _categoryServiceMock.Object,
                 _warehouseCameraServiceMock.Object,
                 _warehouseCategoryServiceMock.Object
             );

            var claims = new[]
            {
                new Claim(ClaimTypes.Role, "admin")
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var principal = new ClaimsPrincipal(identity);
            var httpContext = new DefaultHttpContext
            {
                User = principal
            };

            cameraController.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Act
            var cameraResult = await cameraController.GetCameraByLINQ(1, null, null, null, null, null, null, null);
            var okResult = cameraResult.Result as OkObjectResult;
            var paginationCameraResponse = (cameraResult.Result as OkObjectResult)?.Value as List<PaginationCameraResponse>;

            // Assert
            Assert.NotNull(okResult);
            Assert.NotNull(paginationCameraResponse);
            Assert.Single(paginationCameraResponse);
            Assert.Equal(1, paginationCameraResponse[0].PageIndex);
            Assert.Equal(3, paginationCameraResponse[0].PageSize);
            Assert.Equal(3, paginationCameraResponse[0].Camera.Count);
            Assert.NotNull(cameraList);
        }

        [Fact]
        public async Task GetFromStoredProcedure_Returns_PaginationResponse()
        {
            // Arrange
            var cameraList = await GetTestCameras();
            var pageNumber = 1;
            var pageSize = 3;
            var expectedTotalPages = (int)Math.Ceiling((decimal)cameraList.Count / pageSize);

            _cameraServiceMock.Setup(x => x.GetAllCamera()).ReturnsAsync(cameraList);

            var cameraController = new CamerasController(
                _cameraServiceMock.Object,
                _categoryServiceMock.Object,
                _warehouseCameraServiceMock.Object,
                _warehouseCategoryServiceMock.Object
            );

            var claims = new[]
            {
                new Claim(ClaimTypes.Role, "admin")
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var principal = new ClaimsPrincipal(identity);
            var httpContext = new DefaultHttpContext
            {
                User = principal
            };

            cameraController.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Act
            var result = await cameraController.GetFromStoredProcedure(pageNumber, null, null, null, null);
            var okResult = result.Result as OkObjectResult;
            var paginationCameraResponse = okResult.Value as List<PaginationCameraResponse>;

            // Assert
            Assert.NotNull(okResult);
            Assert.NotNull(paginationCameraResponse);
            Assert.Single(paginationCameraResponse);
            Assert.Equal(pageNumber, paginationCameraResponse[0].PageIndex);
            Assert.Equal(pageSize, paginationCameraResponse[0].PageSize);
            Assert.Equal(expectedTotalPages, paginationCameraResponse[0].TotalPage);
            Assert.Equal(pageSize, paginationCameraResponse[0].Camera.Count);
        }


        [Fact]
        public async Task TestGetCameraBySQL()
        {
            var cameraList = await GetTestCameras();
            var camera = cameraList[1];
            _cameraServiceMock.Setup(x => x.GetAllCamera()).ReturnsAsync(cameraList);
            var cameraController = new CamerasController(_cameraServiceMock.Object,
                _categoryServiceMock.Object,
                _warehouseCameraServiceMock.Object, 
                _warehouseCategoryServiceMock.Object);

            var claims = new[]
            {
                    new Claim(ClaimTypes.Role, "admin")
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var principal = new ClaimsPrincipal(identity);
            var httpContext = new DefaultHttpContext
            {
                User = principal
            };

            cameraController.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            var cameraResult = await cameraController.GetCameraByRawQuery(1);
            var paginationCameraResponse = (cameraResult.Result as OkObjectResult)?.Value as List<PaginationCameraResponse>;
            // Assert
            Assert.NotNull(paginationCameraResponse);
            Assert.Single(paginationCameraResponse);
            Assert.Equal(1, paginationCameraResponse[0].PageIndex);
            Assert.Equal(3, paginationCameraResponse[0].PageSize);
        }

       /* [Fact]
        public async Task TestCreateCamera()
        {
            var newCamera = new Camera
            {
                CameraId = 1,
                Name = "Sony Vip Pro",
                Brand = "Sony",
                CategoryId = 1,
                Description = "description",
                Img = "111",
                Price = 1234,
                Quantity = 1,
                Sold = 1,
                CreatedBy = 1,
                CreatedDate = DateTime.Now,
                UpdatedBy = 1,
                UpdatedDate = DateTime.Now,
                IsDelete = false
            };

            var cameraController = new CamerasController(
                _cameraServiceMock.Object,
                _categoryServiceMock.Object,
                _warehouseCameraServiceMock.Object,
                _warehouseCategoryServiceMock.Object

            var cameraResult = await cameraController.PostCamera(newCamera);

            Assert.NotNull(cameraResult);
            _cameraServiceMock.Verify();
        }*/

        // Fake Data
        private async Task<List<Camera>> GetTestCameras()
        {
            var cameras = new List<Camera>();
            cameras.Add(new Camera { CameraId = 1, Brand = "Sony", CategoryId = 2, Description = "123", Img = "321", Name = "Phucc", Price = 100, Quantity = 20, Sold = 5, CreatedBy = 1, CreatedDate = DateTime.Now, UpdatedBy = 1, UpdatedDate = DateTime.Now, IsDelete = false });
            cameras.Add(new Camera { CameraId = 2, Brand = "Sony", CategoryId = 2, Description = "123", Img = "321", Name = "Phucc", Price = 100, Quantity = 20, Sold = 5, CreatedBy = 1, CreatedDate = DateTime.Now, UpdatedBy = 1, UpdatedDate = DateTime.Now, IsDelete = false });
            cameras.Add(new Camera { CameraId = 3, Brand = "Sony", CategoryId = 2, Description = "123", Img = "321", Name = "Phucc", Price = 100, Quantity = 20, Sold = 5, CreatedBy = 1, CreatedDate = DateTime.Now, UpdatedBy = 1, UpdatedDate = DateTime.Now, IsDelete = false });
            cameras.Add(new Camera { CameraId = 4, Brand = "Sony", CategoryId = 2, Description = "123", Img = "321", Name = "Phucc", Price = 100, Quantity = 20, Sold = 5, CreatedBy = 1, CreatedDate = DateTime.Now, UpdatedBy = 1, UpdatedDate = DateTime.Now, IsDelete = false });

            return cameras;
        }
        private async Task<List<WarehouseCamera>> GetTestWareHouseCameras()
        {
            var cameras = new List<WarehouseCamera>();
            cameras.Add(new WarehouseCamera { CameraId = 1, Brand = "Sony", CategoryId = 2, Description = "123", Img = "321", Name = "Phucc", Price = 100, Quantity = 20, Sold = 5, CreatedBy = 1, CreatedDate = DateTime.Now, UpdatedBy = 1, UpdatedDate = DateTime.Now, IsDelete = false });
            cameras.Add(new WarehouseCamera { CameraId = 2, Brand = "Sony", CategoryId = 2, Description = "123", Img = "321", Name = "Phucc", Price = 100, Quantity = 20, Sold = 5, CreatedBy = 1, CreatedDate = DateTime.Now, UpdatedBy = 1, UpdatedDate = DateTime.Now, IsDelete = false });
            cameras.Add(new WarehouseCamera { CameraId = 3, Brand = "Sony", CategoryId = 2, Description = "123", Img = "321", Name = "Phucc", Price = 100, Quantity = 20, Sold = 5, CreatedBy = 1, CreatedDate = DateTime.Now, UpdatedBy = 1, UpdatedDate = DateTime.Now, IsDelete = false });
            cameras.Add(new WarehouseCamera { CameraId = 4, Brand = "Sony", CategoryId = 2, Description = "123", Img = "321", Name = "Phucc", Price = 100, Quantity = 20, Sold = 5, CreatedBy = 1, CreatedDate = DateTime.Now, UpdatedBy = 1, UpdatedDate = DateTime.Now, IsDelete = false });

            return cameras;
        }
    }
}
