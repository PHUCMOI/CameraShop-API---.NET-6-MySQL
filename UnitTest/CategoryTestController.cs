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
        public async Task TestGetAllCategory()
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
        public async Task TestCreateCategory()
        {
            var newCategory = NewCategory();
            var categoryController = new CategoriesController(_categoryServiceMock.Object);

            var categoryResult = await categoryController.PostCategory(newCategory);

            _categoryServiceMock.Verify(x => x.Create(newCategory), Times.Exactly(1));
        }

        [Fact]
        public async Task TestGetCategoryById()
        {
            var categories = GetTestCategory();
            _categoryServiceMock.Setup(x => x.GetIdAsync(1)).ReturnsAsync(categories[1]);
            var categoryController = new CategoriesController(_categoryServiceMock.Object);

            var categoryResult = await categoryController.GetCategoryByID(1);

            Assert.NotNull(categoryResult);
            Assert.Equal(categories[1].CategoryId, categoryResult.CategoryId);
            Assert.True(categories[1].CategoryId == categoryResult.CategoryId);
        }

        // Fake data
        private List<Category> GetTestCategory()
        {
            var testCategory = new List<Category>();
            testCategory.Add(new Category { CategoryId = 1, Name = "phuc", IsDelete = false, CreatedBy = 1, CreatedDate = DateTime.Now, UpdatedBy = 1, UpdatedDate = DateTime.Now }); ;
            testCategory.Add(new Category { CategoryId = 2, Name = "ư", IsDelete = false, CreatedBy = 1, CreatedDate = DateTime.Now, UpdatedBy = 1, UpdatedDate = DateTime.Now }); ;
            testCategory.Add(new Category { CategoryId = 3, Name = "phưuc", IsDelete = false, CreatedBy = 1, CreatedDate = DateTime.Now, UpdatedBy = 1, UpdatedDate = DateTime.Now }); ;
            testCategory.Add(new Category { CategoryId = 4, Name = "phruc", IsDelete = false, CreatedBy = 1, CreatedDate = DateTime.Now, UpdatedBy = 1, UpdatedDate = DateTime.Now }); ;
            testCategory.Add(new Category { CategoryId = 5, Name = "phytic", IsDelete = false, CreatedBy = 1, CreatedDate = DateTime.Now, UpdatedBy = 1, UpdatedDate = DateTime.Now }); ;

            return testCategory;
        }

        private Category NewCategory()
        {
            return new Category { CategoryId = 1, Name = "phuc", IsDelete = false, CreatedBy = 1, CreatedDate = DateTime.Now, UpdatedBy = 1, UpdatedDate = DateTime.Now };
        }
    }
}