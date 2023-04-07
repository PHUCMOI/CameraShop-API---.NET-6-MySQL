using CameraAPI.Models;
using CameraAPI.Repositories;
using CameraAPI.Services.Interfaces;

namespace CameraAPI.Services
{
    public class CameraService : ICameraService
    {
        public IUnitOfWork _unitOfWork;
        public CameraService(IUnitOfWork unitOfWork) 
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Create(Camera camera)
        {
            if(camera != null)
            {
                await _unitOfWork.Cameras.Create(camera);

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
            if(CameraID > 0)
            {
                var Camera = await _unitOfWork.Cameras.GetById(CameraID);
                if (Camera != null)
                {
                    _unitOfWork.Cameras.Delete(Camera);
                    var result = _unitOfWork.Save();
                    if (result > 0) return true;
                }
            }
            return false;
        }

        public async Task<IEnumerable<Camera>> GetAllCamera()
        {
            var CameraList = await _unitOfWork.Cameras.GetAll();
            return CameraList;
        }

        public async Task<Camera> GetIdAsync(int cameraId)
        {
            if( cameraId > 0 )
            {
                var Camera = await _unitOfWork.Cameras.GetById(cameraId);
                if(Camera != null)
                {
                    return Camera;
                }    
            }
            return null;
        }

        public async Task<bool> Update(Camera camera)
        {
            if(camera != null)
            {
                var cameraDetail = await _unitOfWork.Cameras.GetById(camera.CameraId);
                if(cameraDetail != null)
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

                    _unitOfWork.Cameras.Update(cameraDetail);
                    var result = _unitOfWork.Save();
                    if(result > 0)
                    {
                        return true;
                    }   
                }
            }
            return false;
        }
    }
}
