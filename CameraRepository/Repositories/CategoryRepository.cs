using CameraAPI.Models;

namespace CameraAPI.Repositories
{
    public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
    {
        public CategoryRepository(CameraAPIdbContext dbContext) : base(dbContext)
        {

        }
    }
}
