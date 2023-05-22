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
        public CategoryService(IUnitOfWork unitOfWork, ICategoryRepository categoryRepository, IAutoMapperService autoMapperService)
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

        public async Task<bool> DeleteAsync(int CagetoryID)
        {
            if (CagetoryID > 0)
            {
                var Cagetory = await _categoryRepository.GetById(CagetoryID);
                if (Cagetory != null)
                {
                    _categoryRepository.Delete(Cagetory);
                    var result = _unitOfWork.Save();
                    if (result > 0) return true;
                }
            }
            return false;
        }

        public async Task<IEnumerable<CategoryResponse>> GetAllCategory()
        {
            var categoryList = await _categoryRepository.GetAll();
            var categoryResponseList = _autoMapperService.MapList<Category, CategoryResponse>(categoryList);
            return categoryResponseList;
        }

        public async Task<CategoryResponse> GetIdAsync(int categoryID)
        {
            if (categoryID > 0)
            {
                var Category = await _categoryRepository.GetById(categoryID);
                var categoryResponse = _autoMapperService.Map<Category, CategoryResponse>(Category);

                if (Category != null)
                {
                    return categoryResponse;
                }
            }
            return null;
        }

        public async Task<bool> Update(CategoryResponse categoryResponse, string UserID, int id)
        {
            if (categoryResponse != null)
            {
                var CategoryDetail = await _categoryRepository.GetById(id);
                if (CategoryDetail != null)
                {
                    CategoryDetail.Name = CategoryDetail.Name;
                    CategoryDetail.IsDelete = false;
                    CategoryDetail.UpdatedDate = DateTime.Now;
                    CategoryDetail.CreatedDate = DateTime.Now;
                    CategoryDetail.CreatedBy = Convert.ToInt16(UserID);
                    CategoryDetail.UpdatedBy = Convert.ToInt16(UserID);
                    CategoryDetail.CategoryId = CategoryDetail.CategoryId;

                    _categoryRepository.Update(CategoryDetail);
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
