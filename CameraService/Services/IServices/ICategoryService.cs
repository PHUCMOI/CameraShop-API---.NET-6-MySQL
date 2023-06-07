using CameraAPI.Models;
using CameraCore.Models;

namespace CameraService.Services.IRepositoryServices
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryResponse>> GetAllCategory();
        Task<CategoryResponse> GetIdAsync(int categoryID);
        Task<bool> Create(CategoryRequest category, string userID);
        Task<bool> Update(CategoryRequest categoryResponse, string UserID, int id);
        Task<bool> DeleteAsync(int categoryID);
    }
}
