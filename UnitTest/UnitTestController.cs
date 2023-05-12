using CameraAPI.Controllers;
using CameraAPI.Models;
using CameraAPI.Repositories;
using CameraAPI.Services;
using CameraAPI.Services.Interfaces;
using CameraService.Services.IRepositoryServices;
using Moq;
using Xunit.Sdk;

namespace UnitTest
{
    public class UnitTestController
    {
        private readonly Mock<ICategoryService> _categoryServiceMock;
        private readonly ICategoryService _categoryService;

        public UnitTestController()
        {
            _categoryServiceMock = new Mock<ICategoryService>();
            _categoryService = _categoryServiceMock.Object;
        }

        [Fact]
        public async Task Test1()
        {
            // Arrange
            var productList = GetTestCategory(); // Thay thế Category bằng kiểu dữ liệu tương ứng
            _categoryServiceMock.Setup(x => x.GetAllCategory()).Returns((Delegate)productList);
            var categoryController = new CategoriesController(_categoryServiceMock.Object);

            // Act
            var categoryResult = categoryController.GetCategory();

            // Assert
            Assert.NotNull(categoryResult);
        }

        private IEnumerable<Category> GetTestCategory()
        {
            var testCategory = new List<Category>();
            testCategory.Add(new Category { CategoryId = 1, Name = "phuc", IsDelete = false, CreatedBy = 1, CreatedDate = DateTime.Now, UpdatedBy = 1, UpdatedDate = DateTime.Now }); ;
            testCategory.Add(new Category { CategoryId = 2, Name = "ư", IsDelete = false, CreatedBy = 1, CreatedDate = DateTime.Now, UpdatedBy = 1, UpdatedDate = DateTime.Now }); ;
            testCategory.Add(new Category { CategoryId = 3, Name = "phưuc", IsDelete = false, CreatedBy = 1, CreatedDate = DateTime.Now, UpdatedBy = 1, UpdatedDate = DateTime.Now }); ;
            testCategory.Add(new Category { CategoryId = 4, Name = "phruc", IsDelete = false, CreatedBy = 1, CreatedDate = DateTime.Now, UpdatedBy = 1, UpdatedDate = DateTime.Now }); ;
            testCategory.Add(new Category { CategoryId = 5, Name = "phtyuc", IsDelete = false, CreatedBy = 1, CreatedDate = DateTime.Now, UpdatedBy = 1, UpdatedDate = DateTime.Now }); ;

            return testCategory;
        }
    }
}