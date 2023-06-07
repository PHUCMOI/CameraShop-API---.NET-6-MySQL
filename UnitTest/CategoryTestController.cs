using CameraAPI.Controllers;
using CameraAPI.Models;
using CameraAPI.Repositories;
using CameraAPI.Services;
using CameraAPI.Services.Interfaces;
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
            _categoryServiceMock.Setup(x => x.GetAllCategory()).ReturnsAsync(GetTestCategory());
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

            _categoryServiceMock.Verify(x => x.Create(newCategory), Times.Exactly(1));
        }

        [Fact]
        public async Task TestGetCategoryById_Return200()
        {
            var categories = GetTestCategory();
            _categoryServiceMock.Setup(x => x.GetIdAsync(1)).ReturnsAsync(categories[1]);
            var categoryController = new CategoriesController(_categoryServiceMock.Object);

            var categoryResult = await categoryController.GetCategoryByID(1);

            Assert.NotNull(categoryResult);
            Assert.Equal(categories[1].CategoryId, categoryResult.CategoryId);
            Assert.True(categories[1].CategoryId == categoryResult.CategoryId);
        }

        [Fact]
        public async Task TestUpdateCategory_ReturnsOkWithTrueValue()
        {
            // Arrange
            var category = NewCategory();

            _categoryServiceMock.Setup(x => x.Update(category))
                .ReturnsAsync(true);

            var categoryController = new CategoriesController(_categoryServiceMock.Object);


            // Act
            var result = await categoryController.PutCategory(category.CategoryId, category);

            // Assert
            Assert.IsType<OkObjectResult>(result);

            var actionResult = Assert.IsType<OkObjectResult>(result);
            Assert.IsType<bool>(actionResult.Value);
            Assert.True((bool)actionResult.Value);

            _categoryServiceMock.Verify(x => x.Update(category), Times.Once);
        }

        // Fake data
        private List<User> GetTestCategory()
        {
            var testCategory = new List<User>();
            testCategory.Add(new User { CategoryId = 1, Name = "phuc", IsDelete = false, CreatedBy = 1, CreatedDate = DateTime.Now, UpdatedBy = 1, UpdatedDate = DateTime.Now }); ;
            testCategory.Add(new User { CategoryId = 2, Name = "ư", IsDelete = false, CreatedBy = 1, CreatedDate = DateTime.Now, UpdatedBy = 1, UpdatedDate = DateTime.Now }); ;
            testCategory.Add(new User { CategoryId = 3, Name = "phưuc", IsDelete = false, CreatedBy = 1, CreatedDate = DateTime.Now, UpdatedBy = 1, UpdatedDate = DateTime.Now }); ;
            testCategory.Add(new User { CategoryId = 4, Name = "phruc", IsDelete = false, CreatedBy = 1, CreatedDate = DateTime.Now, UpdatedBy = 1, UpdatedDate = DateTime.Now }); ;
            testCategory.Add(new User { CategoryId = 5, Name = "phytic", IsDelete = false, CreatedBy = 1, CreatedDate = DateTime.Now, UpdatedBy = 1, UpdatedDate = DateTime.Now }); ;

            return testCategory;
        }

        private User NewCategory()
        {
            return new User { CategoryId = 1, Name = "phuc", IsDelete = false, CreatedBy = 1, CreatedDate = DateTime.Now, UpdatedBy = 1, UpdatedDate = DateTime.Now };
        }
    }
}