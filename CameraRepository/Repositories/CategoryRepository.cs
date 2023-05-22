using CameraAPI.Models;
using CameraCore.Models;
using static Dapper.SqlMapper;

namespace CameraAPI.Repositories
{
    public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
    {
        public CategoryRepository(CameraAPIdbContext dbContext) : base(dbContext)
        {

        }
    }
}
