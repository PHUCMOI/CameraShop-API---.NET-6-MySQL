using CameraAPI.AppModel;
using CameraAPI.Models;

namespace CameraService.Services.IRepositoryServices
{
    public interface IOrderService 
    {
        Task<OrderRequest> GetRandomOrder();
        Task<IEnumerable<Order>> GetAllOrder();
        Task<Order> GetIdAsync(int OrderID);
        Task<bool> Create(Order order);
        Task<bool> Update(Order order);
        Task<bool> DeleteAsync(int OrderID);
    }
}
