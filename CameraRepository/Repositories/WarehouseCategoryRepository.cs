using CameraAPI.Models;
using System.Drawing.Text;

namespace CameraAPI.Repositories
{
    public class WarehouseCategoryRepository : GenericRepository<WarehouseCategory>, IWarehouseCategoryRepository
    {
        public WarehouseCategoryRepository(WarehouseDbContext dbContext) : base(dbContext)
        {

        }
    }
}
