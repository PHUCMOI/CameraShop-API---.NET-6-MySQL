using CameraAPI.Models;
using System.Drawing.Text;

namespace CameraAPI.Repositories
{
    public class WarehouseCameraRepository : GenericRepository<WarehouseCamera>, IWarehouseCameraRepository
    {
        public WarehouseCameraRepository(WarehouseDbContext dbContext) : base(dbContext)
        {
            
        }

    }
}
