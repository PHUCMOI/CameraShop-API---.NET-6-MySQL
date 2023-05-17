using CameraAPI.Models;
using CameraAPI.Repositories;
using CameraAPI.Services.Interfaces;
using CameraService.Services.IRepositoryServices;

namespace CameraAPI.Services
{
    public class CategoryService : ICategoryService
    {
        public IUnitOfWork _unitOfWork;
        private readonly ICategoryRepository _categoryRepository;
        public CategoryService(IUnitOfWork unitOfWork, ICategoryRepository categoryRepository)
        {
            _unitOfWork = unitOfWork;
            _categoryRepository = categoryRepository;
        }

        public async Task<bool> Create(Category category)
        {
            if (category != null)
            {
                await _unitOfWork.Categories.Create(category);

                //Lưu xuống db 
                var result = _unitOfWork.Save();

                if (result > 0)
                    return true;
                else
                    return true;
            }
            return false;
        }

        public async Task<bool> DeleteAsync(int categoryID)
        {
            if (categoryID > 0)
            {
                var category = await _unitOfWork.Categories.GetById(categoryID);
                if (category != null)
                {
                    _unitOfWork.Categories.Delete(category);
                    var result = _unitOfWork.Save();
                    if (result > 0) return true;
                }
            }
            return false;
        }

        public async Task<IEnumerable<Category>> GetAllCategory()
        {
            var CategoryList = await _categoryRepository.GetAll();
            return CategoryList;
        }

        public async Task<Category> GetIdAsync(int categoryID)
        {
            if (categoryID > 0)
            {
                var Category = await _categoryRepository.GetById(categoryID);
                if (Category != null)
                {
                    return Category;
                }
            }
            return null;
        }

        public async Task<bool> Update(Category Category)
        {
            if (Category != null)
            {
                var CategoryDetail = await _unitOfWork.Categories.GetById(Category.CategoryId);
                if (CategoryDetail != null)
                {
                    CategoryDetail.Name = Category.Name;
                    CategoryDetail.IsDelete = Category.IsDelete;
                    CategoryDetail.UpdatedDate = Category.UpdatedDate;
                    CategoryDetail.CreatedDate = Category.CreatedDate;
                    CategoryDetail.CreatedBy = Category.CreatedBy;
                    CategoryDetail.UpdatedBy = Category.UpdatedBy;
                    CategoryDetail.CategoryId = Category.CategoryId;

                    _unitOfWork.Categories.Update(CategoryDetail);
                    var result = _unitOfWork.Save();
                    if (result > 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
