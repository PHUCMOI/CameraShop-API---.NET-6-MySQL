using CameraAPI.AppModel;
using CameraAPI.Models;

namespace CameraAPI.Services.Interfaces
{
    public interface ICameraService
    {
        Task<List<Camera>> GetAllCamera();
        Task<Camera> GetIdAsync(int cameraId);
        Task<List<PaginationCameraResponse>> GetCameraByLINQ(int pageNumber, int? categoryID = null,
            string? name = null, string? brand = null, decimal? minPrice = null, decimal? maxPrice = null,
        string? FilterType = null, int? quantity = null);
        Task<List<PaginationCameraResponse>> GetCameraBySQL(int pageNumber, int? categoryID = null,
            string? name = null, string? brand = null, decimal? minPrice = null, decimal? maxPrice = null,
        string? FilterType = null, int? quantity = null);
        Task<List<PaginationCameraResponse>> GetFromStoredProcedure(int pageNumber, int? categoryID = null, string? name = null,
        string? brand = null, decimal? minPrice = null, decimal? maxPrice = null, int? quantity = null);
        Task<bool> Create(Camera camera);
        Task<bool> Update(Camera camera);
        Task<bool> DeleteAsync(int CameraID);        
    }
}
