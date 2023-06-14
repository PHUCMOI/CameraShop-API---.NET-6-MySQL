using CameraAPI.Models;
using CameraAPI.Repositories;
using CameraCore.Models;
using CameraService.Services.IRepositoryServices;
using CameraService.Services.IServices;

namespace CameraAPI.Services
{
    public class CategoryService : ICategoryService
    {
        private IUnitOfWork _unitOfWork;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IAutoMapperService _autoMapperService;
        public CategoryService(IUnitOfWork unitOfWork, 
            ICategoryRepository categoryRepository, 
            IAutoMapperService autoMapperService)
        {
            _unitOfWork = unitOfWork;
            _categoryRepository = categoryRepository;
            _autoMapperService = autoMapperService;
        }

        public async Task<bool> Create(CategoryRequest categoryRequest, string userID)
        {
            if (categoryRequest != null)
            {
                var category = new Category()
                {
                    Name = categoryRequest.Name,
                    UpdatedBy = Convert.ToInt16(userID),
                    UpdatedDate = DateTime.Now,
                    CreatedBy = Convert.ToInt16(userID),
                    CreatedDate = DateTime.Now,
                    IsDelete = false
                };

                await _categoryRepository.Create(category);

                //Lưu xuống db 
                var result = _unitOfWork.Save();

                if (result > 0)
                    return true;
                else
                    return true;
            }
            return false;
        }

        public Task<bool> DeleteAsync(int cagetoryID)
        {
            if (cagetoryID > 0)
            {
                var cagetory = _categoryRepository.Delete(cagetoryID);
                if (cagetory)
                {
                    var result = _unitOfWork.Save();
                    if (result == 0) return Task.FromResult(true);
                }
            }
            return Task.FromResult(false);
        }

        public async Task<IEnumerable<CategoryResponse>> GetAllCategory()
        {
            var categoryList = await _categoryRepository.GetCategoryList();
            var categoryResponseList = _autoMapperService.MapList<Category, CategoryResponse>(categoryList);
            return categoryResponseList;
        }

        public async Task<CategoryResponse> GetIdAsync(int categoryID)
        {
            if (categoryID > 0)
            {
                var category = await _categoryRepository.GetById(categoryID);
                if (category.IsDelete == true)
                {
                    return null;
                }    
                var categoryResponse = _autoMapperService.Map<Category, CategoryResponse>(category);

                if (categoryResponse != null)
                {
                    return categoryResponse;
                }
            }
            throw new Exception();
        }

        public async Task<bool> Update(CategoryRequest categoryRequest, string UserID, int id)
        {
            if (categoryRequest != null)
            {
                var categoryDetail = await _categoryRepository.GetById(id);
                if (categoryDetail != null)
                {
                    categoryDetail.Name = categoryRequest.Name;
                    categoryDetail.IsDelete = false;
                    categoryDetail.UpdatedDate = DateTime.Now;
                    categoryDetail.CreatedDate = categoryDetail.CreatedDate;
                    categoryDetail.CreatedBy = categoryDetail.CreatedBy;
                    categoryDetail.UpdatedBy = Convert.ToInt16(UserID);
                    categoryDetail.CategoryId = categoryDetail.CategoryId;

                    _categoryRepository.Update(categoryDetail);
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
