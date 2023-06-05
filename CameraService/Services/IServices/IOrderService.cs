using CameraAPI.AppModel;
using CameraAPI.Models;

namespace CameraService.Services.IRepositoryServices
{
    public interface IOrderService 
    {
        Task<OrderRequestPayPal> GetRandomOrder();
        Task<IEnumerable<OrderResponse>> GetAllOrder();
        Task<Order> GetIdAsync(int OrderID);
        Task<bool> Create(OrderRequest order, CameraResponse camera, string UserID, decimal Quantity);
        Task<bool> Update(Order order);
        Task<bool> DeleteAsync(int OrderID);
    }
}
