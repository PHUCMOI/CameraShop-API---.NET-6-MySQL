using CameraAPI.Models;
using CameraAPI.Repositories;
using CameraCore.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CameraRepository.Repositories
{
    public class OrderRepository : GenericRepository<Order>, IOrderRepository
    {
        public OrderRepository(CameraAPIdbContext dbContext) : base(dbContext)
        {

        }
    }
}
