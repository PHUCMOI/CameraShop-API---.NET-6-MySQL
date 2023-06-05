using CameraAPI.Models;
using CameraAPI.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CameraRepository.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(CameraAPIdbContext dbContext) : base(dbContext)
        {

        }
    }
}
