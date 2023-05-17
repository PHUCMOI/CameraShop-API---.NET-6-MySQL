using CameraAPI.Models;

namespace CameraService.Services.IRepositoryServices
{
    public interface ICategoryService
    {
        Task<IEnumerable<Category>> GetAllCategory();
        Task<Category> GetIdAsync(int categoryID);
        Task<bool> Create(Category category);
        Task<bool> Update(Category category);
        Task<bool> DeleteAsync(int categoryID);
    }
}
