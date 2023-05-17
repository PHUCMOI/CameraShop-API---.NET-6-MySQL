using CameraAPI.Models;
using CameraAPI.Repositories;
using CameraAPI.Services.Interfaces;

namespace CameraAPI.Services
{
    public class WarehouseCameraService : IWarehouseCameraService
    {
        public IUnitOfWork _unitOfWork;
        public IWarehouseCameraRepository _warehouseCameraRepository;
        public WarehouseCameraService(IUnitOfWork unitOfWork, IWarehouseCameraRepository warehouseCameraRepository)
        {
            _unitOfWork = unitOfWork;
            _warehouseCameraRepository = warehouseCameraRepository;
        }

        public async Task<bool> Create(WarehouseCamera camera)
        {
            if (camera != null)
            {
                await _unitOfWork.WarehouseCamera.Create(camera);

                //Lưu xuống db 
                var result = _unitOfWork.Save();

                if (result > 0)
                    return true;
                else
                    return true;
            }
            return false;
        }

        public async Task<bool> DeleteAsync(int CameraID)
        {
            if (CameraID > 0)
            {
                var Camera = await _unitOfWork.WarehouseCamera.GetById(CameraID);
                if (Camera != null)
                {
                    _unitOfWork.WarehouseCamera.Delete(Camera);
                    var result = _unitOfWork.Save();
                    if (result > 0) return true;
                }
            }
            return false;
        }

        public async Task<IEnumerable<WarehouseCamera>> GetAllCamera()
        {
            var CameraList = await _warehouseCameraRepository.GetAllWarehouse();
            return CameraList;
        }

        public async Task<WarehouseCamera> GetIdAsync(int cameraId)
        {
            if (cameraId > 0)
            {
                var Camera = await _unitOfWork.WarehouseCamera.GetById(cameraId);
                if (Camera != null)
                {
                    return Camera;
                }
            }
            return null;
        }

        public async Task<bool> Update(WarehouseCamera camera)
        {
            if (camera != null)
            {
                var cameraDetail = await _unitOfWork.WarehouseCamera.GetById(camera.CameraId);
                if (cameraDetail != null)
                {
                    cameraDetail.Name = camera.Name;
                    cameraDetail.Description = camera.Description;
                    cameraDetail.IsDelete = camera.IsDelete;
                    cameraDetail.UpdatedDate = camera.UpdatedDate;
                    cameraDetail.CreatedDate = camera.CreatedDate;
                    cameraDetail.CreatedBy = camera.CreatedBy;
                    cameraDetail.UpdatedBy = camera.UpdatedBy;
                    cameraDetail.Brand = camera.Brand;
                    cameraDetail.CategoryId = camera.CategoryId;
                    cameraDetail.Img = camera.Img;
                    cameraDetail.Price = camera.Price;
                    cameraDetail.Quantity = camera.Quantity;

                    _unitOfWork.WarehouseCamera.Update(cameraDetail);
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
