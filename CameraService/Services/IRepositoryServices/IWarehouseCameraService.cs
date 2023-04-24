using CameraAPI.Models;

namespace CameraAPI.Services.Interfaces
{
    public interface IWarehouseCameraService
    {
        Task<IEnumerable<WarehouseCamera>> GetAllCamera();
        Task<WarehouseCamera> GetIdAsync(int cameraId);
        Task<bool> Create(WarehouseCamera camera);
        Task<bool> Update(WarehouseCamera camera);
        Task<bool> DeleteAsync(int CameraID);
    }
}
