using CameraAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CameraService.Services.IRepositoryServices
{
    public interface IWarehouseCategoryService
    {
        Task<IEnumerable<WarehouseCategory>> GetAllCategory();
        Task<WarehouseCategory> GetIdAsync(int cameraId);
        Task<bool> Create(WarehouseCategory camera);
        Task<bool> Update(WarehouseCategory camera);
        Task<bool> DeleteAsync(int CameraID);
    }
}
