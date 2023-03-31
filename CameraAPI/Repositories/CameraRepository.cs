using CameraAPI.Models;

namespace CameraAPI.Repositories
{
    public class CameraRepository : GenericRepository<Camera>, ICameraRepository
    {
        public CameraRepository(CameraAPIdbContext dbContext) : base(dbContext)
        {

        }
    }
}
