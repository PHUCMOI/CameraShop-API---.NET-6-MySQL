using CameraAPI.Models;

namespace CameraAPI.Services.Interfaces
{
    public interface ICameraService
    {
        Task<IEnumerable<Camera>> GetAllCamera();
        Task<Camera> GetIdAsync(int cameraId);
        Task<bool> Create(Camera camera);
        Task<bool> Update(Camera camera);
        Task<bool> DeleteAsync(int CameraID);        
    }
}
