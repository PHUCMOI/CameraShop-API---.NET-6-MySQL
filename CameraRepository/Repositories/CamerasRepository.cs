using CameraAPI.Models;

namespace CameraAPI.Repositories
{
    public class CamerasRepository : GenericRepository<Camera>, ICameraRepository
    {
        public CamerasRepository(CameraAPIdbContext dbContext) : base(dbContext)
        {
            
        }
    }
}