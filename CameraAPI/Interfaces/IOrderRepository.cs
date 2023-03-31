using CameraAPI.Models;

namespace CameraAPI.Repositories
{
    public interface IOrderRepository : IGenericRepository<Order>
    {
        void Add(Order order);
    }
}
