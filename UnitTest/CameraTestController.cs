using CameraAPI.AppModel;
using CameraAPI.Controllers;
using CameraAPI.Models;
using CameraAPI.Services.Interfaces;
using CameraService.Services.IRepositoryServices;
using Microsoft.AspNetCore.Mvc;
using Moq;

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
        public async Task GetAllCamera_Return_Default()
        {
            var cameraList = await GetTestCameras();
            //_cameraServiceMock.Setup(x => x.GetAllCamera()).ReturnsAsync(cameraList);
            var _camerasController = new CamerasController(_cameraServiceMock.Object,
                    _categoryServiceMock.Object,
                    _warehouseCameraServiceMock.Object,
                    _warehouseCategoryServiceMock.Object);
            var cameraResult = await _camerasController.GetCameras();

            Assert.NotNull(cameraResult);
        }

        [Fact]
        public async Task GetAllCameraByLINQ_Returns_PaginationResponse()
        {
            // Arrange
            var cameraList = await GetTestCameras();
            var pageNumber = 1;
            var pageSize = 3;
            var expectedTotalPages = (int)Math.Ceiling((decimal)cameraList.Count / pageSize);

            var cameraResponseList = MapCameraToCameraResponse(cameraList);

            _cameraServiceMock.Setup(x => x.GetCameraByLINQ(pageNumber, null, null, null, null, null, null, null))
                .ReturnsAsync(MapCameraResponse(cameraResponseList, pageNumber));

            var camerasController = new CamerasController(_cameraServiceMock.Object,
                _categoryServiceMock.Object,
                _warehouseCameraServiceMock.Object,
                _warehouseCategoryServiceMock.Object);

            // Act
            var result = await camerasController.GetCameraByLINQ(pageNumber, null, null, null, null, null, null, null);

            // Assert
            var actionResult = Assert.IsType<ActionResult<PaginationCameraResponse>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var paginationResponse = Assert.IsType<List<PaginationCameraResponse>>(okResult.Value);


            Assert.NotNull(result);
            Assert.NotNull(actionResult);
            Assert.NotNull(okResult);
            Assert.NotNull(paginationResponse);
            Assert.Equal(cameraResponseList[0].CameraName, paginationResponse[0].Camera[0].CameraName);
            Assert.Equal(pageSize, paginationResponse[0].PageSize);
            Assert.Equal(pageSize, paginationResponse[0].Camera.Count);
        }

        [Fact]
        public async Task GetFromStoredProcedure_Returns_PaginationResponse()
        {
            // Arrange
            var cameraList = await GetTestCameras(); 
            var pageNumber = 1;
            var pageSize = 3;
            var expectedTotalPages = (int)Math.Ceiling((decimal)cameraList.Count / pageSize);

            var cameraResponseList = MapCameraToCameraResponse(cameraList);

            /*_cameraServiceMock.Setup(x => x.GetFromStoredProcedure(pageNumber, null, null, null, null, null, null))
                .ReturnsAsync(MapCameraResponse(cameraResponseList, pageNumber)); */
           
            var _camerasController = new CamerasController(_cameraServiceMock.Object,
                    _categoryServiceMock.Object,
                    _warehouseCameraServiceMock.Object,
                    _warehouseCategoryServiceMock.Object);
            // Act
            var result = await _camerasController.GetFromStoredProcedure(pageNumber, null, null, null, null, null, null);

            // Assert
            var actionResult = Assert.IsType<ActionResult<List<PaginationCameraResponse>>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var paginationResponse = Assert.IsType<List<PaginationCameraResponse>>(okResult.Value);

            Assert.NotNull(result);
            Assert.NotNull(actionResult);
            Assert.NotNull(okResult);
            Assert.NotNull(paginationResponse);
            Assert.Equal(cameraResponseList[0].CameraName, paginationResponse[0].Camera[0].CameraName);
            Assert.Equal(pageSize, paginationResponse[0].PageSize);
            Assert.Equal(pageSize, paginationResponse[0].Camera.Count);
        }

        [Fact]
        public async Task GetAllCameraBySQL_Returns_PaginationResponse()
        {
            // Arrange
            var cameraList = await GetTestCameras();
            var pageNumber = 1;
            var pageSize = 3;
            var expectedTotalPages = (int)Math.Ceiling((decimal)cameraList.Count / pageSize);

            var cameraResponseList = MapCameraToCameraResponse(cameraList);

            _cameraServiceMock.Setup(x => x.GetCameraBySQL(pageNumber, null, null, null, null, null, null, null))
                .ReturnsAsync(MapCameraResponse(cameraResponseList, pageNumber));

            var _camerasController = new CamerasController(_cameraServiceMock.Object,
                    _categoryServiceMock.Object,
                    _warehouseCameraServiceMock.Object,
                    _warehouseCategoryServiceMock.Object);
            // Act
            var result = await _camerasController.GetCameraByRawQuery(pageNumber, null, null, null, null, null, null, null);

            // Assert
            var actionResult = Assert.IsType<ActionResult<List<PaginationCameraResponse>>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var paginationResponse = Assert.IsType<List<PaginationCameraResponse>>(okResult.Value);

            Assert.NotNull(result);
            Assert.NotNull(actionResult);
            Assert.NotNull(okResult);
            Assert.NotNull(paginationResponse);
            Assert.Equal(cameraResponseList[0].CameraName, paginationResponse[0].Camera[0].CameraName);
            Assert.Equal(pageSize, paginationResponse[0].PageSize);
            Assert.Equal(pageSize, paginationResponse[0].Camera.Count);
        }

        [Fact]
        public async Task TestCreateNewCamera_Return_200()
        {
            // Arrange
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

            //_cameraServiceMock.Setup(x => x.Create(newCamera))
             //   .ReturnsAsync(true);

            var camerasController = new CamerasController(_cameraServiceMock.Object,
                _categoryServiceMock.Object,
                _warehouseCameraServiceMock.Object,
                _warehouseCategoryServiceMock.Object);

            // Act
            //var result = await camerasController.PostCamera(newCamera);

            // Assert
            //var actionResult = Assert.IsType<ActionResult<Camera>>(result);
            //var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            //var cameraDetail = Assert.IsType<bool>(okResult.Value);

            //Assert.True(cameraDetail);
            //_cameraServiceMock.Verify(x => x.Create(newCamera), Times.Once);
        }

        [Fact]
        public async Task TestUpdateCamera_ReturnsOkWithTrueValue()
        {
            // Arrange
            var camera = new Camera
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

            //_cameraServiceMock.Setup(x => x.Update(camera.id))
               // .ReturnsAsync(true);

            var camerasController = new CamerasController(_cameraServiceMock.Object,
                _categoryServiceMock.Object,
                _warehouseCameraServiceMock.Object,
                _warehouseCategoryServiceMock.Object);

            // Act
            //var result = await camerasController.PutCamera(camera);

            // Assert
            //Assert.IsType<OkObjectResult>(result);

            //var actionResult = Assert.IsType<OkObjectResult>(result);
            //Assert.IsType<bool>(actionResult.Value);
            //Assert.True((bool)actionResult.Value);

            //_cameraServiceMock.Verify(x => x.Update(camera), Times.Once);
        }



        // Fake Data
        private async Task<List<Camera>> GetTestCameras()
        {
            var cameras = new List<Camera>();
            cameras.Add(new Camera { CameraId = 1, Brand = "Sony", CategoryId = 2, Description = "123", Img = "321", Name = "Phucc1", Price = 100, Quantity = 20, Sold = 5, CreatedBy = 1, CreatedDate = DateTime.Now, UpdatedBy = 1, UpdatedDate = DateTime.Now, IsDelete = false });
            cameras.Add(new Camera { CameraId = 2, Brand = "Sony", CategoryId = 2, Description = "123", Img = "321", Name = "Phucc2", Price = 100, Quantity = 20, Sold = 5, CreatedBy = 1, CreatedDate = DateTime.Now, UpdatedBy = 1, UpdatedDate = DateTime.Now, IsDelete = false });
            cameras.Add(new Camera { CameraId = 3, Brand = "Sony", CategoryId = 2, Description = "123", Img = "321", Name = "Phucc3", Price = 100, Quantity = 20, Sold = 5, CreatedBy = 1, CreatedDate = DateTime.Now, UpdatedBy = 1, UpdatedDate = DateTime.Now, IsDelete = false });
            cameras.Add(new Camera { CameraId = 4, Brand = "Sony", CategoryId = 2, Description = "123", Img = "321", Name = "Phucc4", Price = 100, Quantity = 20, Sold = 5, CreatedBy = 1, CreatedDate = DateTime.Now, UpdatedBy = 1, UpdatedDate = DateTime.Now, IsDelete = false });

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
        private List<PaginationCameraResponse> MapCameraResponse(List<CameraResponse> cameras, int pageNumber)
        {
            var cameraList = cameras.ToList();
            var count = cameraList.Count;
            var pageSize = 3;
            var totalPage = (int)Math.Ceiling((decimal)count / pageSize);
            if (pageNumber == 0) pageNumber = 1;

            var paginationResponse = new PaginationCameraResponse
            {
                Camera = cameraList.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList(),
                PageIndex = pageNumber,
                PageSize = pageSize,
                TotalPage = totalPage
            };

            return new List<PaginationCameraResponse> { paginationResponse };
        }

        private List<CameraResponse> MapCameraToCameraResponse(List<Camera> cameras)
        {
            if (cameras == null)
            {
                return null;
            }

            return cameras.Select(camera => new CameraResponse
            {
                CameraName = camera.Name,
                Brand = camera.Brand,
                Price = camera.Price,
                Img = camera.Img,
                Quantity = camera.Quantity,
                Description = camera.Description,
                CategoryName = camera.CategoryId.ToString(),
                BestSeller = camera.Sold.ToString()
            }).ToList();
        }
    }
}
