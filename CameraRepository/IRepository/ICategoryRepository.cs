using CameraAPI.Models;
using CameraCore.Models;

namespace CameraAPI.Repositories
{
    public interface ICategoryRepository : IGenericRepository<Category>
    {
        Task<List<Category>> GetCategoryList();
        bool Delete(int categoryId);
    }
}
