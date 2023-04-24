using CameraAPI.Models;
using CameraAPI.Repositories;
using CameraCore.IRepository;
namespace CameraRepository.Repositories
{
    internal class WarehouseCategoryRepository : GenericRepository<WarehouseCategory>, IWarehouseCategoryRepository
    {
        public WarehouseCategoryRepository(WarehouseDbContext dbContext) : base(dbContext)
        {

        }
    }
}
