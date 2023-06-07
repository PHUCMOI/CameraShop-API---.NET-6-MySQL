using CameraAPI.AppModel;
using CameraAPI.Models;

namespace CameraAPI.Repositories
{
    public interface ICameraRepository : IGenericRepository<Camera>
    {
        Task<List<Camera>> GetCameraList();
        Task<List<CameraResponse>> GetBySQL(int pageNumber, int? categoryID = null, string? name = null, string? brand = null, decimal? minPrice = null, decimal? maxPrice = null, string? FilterType = null, int? quantity = null);
        Task<List<CameraResponse>> GetByStoredProcedure(int pageNumber, int? categoryID = null, string? name = null, string? brand = null, decimal? minPrice = null, decimal? maxPrice = null,string? FilterType = null, int? quantity = null);
        bool Delete(int cameraId);
    }
}
