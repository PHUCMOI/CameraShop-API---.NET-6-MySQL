using CameraAPI.Controllers;
using CameraAPI.Models;
using CameraAPI.Repositories;
using CameraAPI.Services;
using CameraAPI.Services.Interfaces;
using CameraCore.Models;
using CameraService.Services.IRepositoryServices;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit.Sdk;

namespace UnitTest
{
    public class CategoryTestController
    {
        private readonly Mock<ICategoryService> _categoryServiceMock;
        public CategoryTestController()
        {
            _categoryServiceMock = new Mock<ICategoryService>();
        }

        [Fact]
        public async Task TestGetAllCategory_Return200()
        {
            // Arrange
            //_categoryServiceMock.Setup(x => x.GetAllCategory()).ReturnsAsync(GetTestCategory());
            var categoryController = new CategoriesController(_categoryServiceMock.Object);

            // Act
            var categoryResult = (OkObjectResult)await categoryController.GetCategory();

            // Assert
            Assert.NotNull(categoryResult);
        }

        [Fact]
        public async Task TestCreateCategory_Return200()
        {
            var newCategory = NewCategory();
            var categoryController = new CategoriesController(_categoryServiceMock.Object);

            var categoryResult = await categoryController.PostCategory(newCategory);

            _categoryServiceMock.Verify(x => x.Create(newCategory, "1"), Times.Exactly(1));
        }

        [Fact]
        public async Task TestGetCategoryById_Return200()
        {
            var categories = GetTestCategory();
            //_categoryServiceMock.Setup(x => x.GetIdAsync(1)).ReturnsAsync(categories[1]);
            var categoryController = new CategoriesController(_categoryServiceMock.Object);

            var categoryResult = await categoryController.GetCategoryByID(1);

            Assert.NotNull(categoryResult);
            //Assert.Equal(categories[1].CategoryId, categoryResult.CategoryId);
            //Assert.True(categories[1].CategoryId == categoryResult.CategoryId);
        }

        [Fact]
        public async Task TestUpdateCategory_ReturnsOkWithTrueValue()
        {
            // Arrange
            var category = NewCategory();

            _categoryServiceMock.Setup(x => x.Update(category, "1", 1))
                .ReturnsAsync(true);

            var categoryController = new CategoriesController(_categoryServiceMock.Object);


            // Act
            var result = await categoryController.PutCategory(category, 1);

            // Assert
            Assert.IsType<OkObjectResult>(result);

            var actionResult = Assert.IsType<OkObjectResult>(result);
            Assert.IsType<bool>(actionResult.Value);
            Assert.True((bool)actionResult.Value);

            _categoryServiceMock.Verify(x => x.Update(category, "1", 1), Times.Once);
        }

        // Fake data
        private List<CategoryRequest> GetTestCategory()
        {
            var testCategory = new List<CategoryRequest>();
            testCategory.Add(new CategoryRequest { Name = "phuc"}); ;
            testCategory.Add(new CategoryRequest {Name = "ư" }); ;
            testCategory.Add(new CategoryRequest {Name = "phưuc" }); ;
            testCategory.Add(new CategoryRequest {Name = "phruc" }); ;
            testCategory.Add(new CategoryRequest {Name = "phytic" }); ;

            return testCategory;
        }

        private CategoryRequest NewCategory()
        {
            return new CategoryRequest {Name = "phuc" };
        }
    }
}