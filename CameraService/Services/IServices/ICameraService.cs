using CameraAPI.AppModel;
using CameraAPI.Models;
using CameraCore.Models;

namespace CameraAPI.Services.Interfaces
{
    public interface ICameraService
    {
        Task<List<CameraResponse>> GetAllCamera();
        Task<CameraResponseID> GetIdAsync(int cameraId);
        Task<List<PaginationCameraResponse>> GetCameraByLINQ(int pageNumber, int? categoryID = null,
            string? name = null, string? brand = null, decimal? minPrice = null, decimal? maxPrice = null,
        string? FilterType = null, int? quantity = null);
        Task<List<PaginationCameraResponse>> GetCameraBySQL(int pageNumber, int? categoryID = null,
            string? name = null, string? brand = null, decimal? minPrice = null, decimal? maxPrice = null,
        string? FilterType = null, int? quantity = null);
        Task<List<PaginationCameraResponse>> GetFromStoredProcedure(int pageNumber, int? categoryID = null, string? name = null,
        string? brand = null, decimal? minPrice = null, decimal? maxPrice = null,string? FilterType = null, int? quantity = null);
        Task<bool> Create(CameraPostRequest cameraPostRequest, string UserID);
        Task<bool> Update(CameraResponse camera, string UserID, int id);
        Task<bool> DeleteAsync(int CameraID);        
    }
}
