using CameraAPI.Models;
using CameraAPI.Repositories;
using CameraAPI.Services.Interfaces;
using CameraCore.IRepository;
using CameraService.Services.IRepositoryServices;

namespace CameraAPI.Services
{
    public class WarehouseCategoryService : IWarehouseCategoryService
    {
        public IUnitOfWork _unitOfWork;
        public IWarehouseCategoryRepository _warehouseCategory;
        public WarehouseCategoryService(IUnitOfWork unitOfWork, IWarehouseCategoryRepository warehouseCategory)
        {
            _unitOfWork = unitOfWork;
            _warehouseCategory = warehouseCategory;
        }

        public async Task<bool> Create(WarehouseCategory category)
        {
            if (category != null)
            {
                await _unitOfWork.WarehouseCategory.Create(category);

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
                var category = await _unitOfWork.WarehouseCategory.GetById(categoryID);
                if (category != null)
                {
                    _unitOfWork.WarehouseCategory.Delete(category);
                    var result = _unitOfWork.Save();
                    if (result > 0) return true;
                }
            }
            return false;
        }

        public async Task<IEnumerable<WarehouseCategory>> GetAllCategory()
        {
            var CategoryList = await _warehouseCategory.GetAllWarehouse();
            return CategoryList;
        }

        public async Task<WarehouseCategory> GetIdAsync(int categoryID)
        {
            if (categoryID > 0)
            {
                var Category = await _unitOfWork.WarehouseCategory.GetById(categoryID);
                if (Category != null)
                {
                    return Category;
                }
            }
            return null;
        }

        public async Task<bool> Update(WarehouseCategory Category)
        {
            if (Category != null)
            {
                var CategoryDetail = await _unitOfWork.WarehouseCategory.GetById(Category.CategoryId);
                if (CategoryDetail != null)
                {
                    CategoryDetail.Name = Category.Name;
                    CategoryDetail.IsDelete = Category.IsDelete;
                    CategoryDetail.UpdatedDate = Category.UpdatedDate;
                    CategoryDetail.CreatedDate = Category.CreatedDate;
                    CategoryDetail.CreatedBy = Category.CreatedBy;
                    CategoryDetail.UpdatedBy = Category.UpdatedBy;
                    CategoryDetail.CategoryId = Category.CategoryId;

                    _unitOfWork.WarehouseCategory.Update(CategoryDetail);
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
