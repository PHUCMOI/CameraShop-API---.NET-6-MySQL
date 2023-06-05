using CameraAPI.AppModel;
using CameraAPI.Models;

namespace CameraService.Services.IRepositoryServices
{
    public interface IOrderService 
    {
        Task<IEnumerable<OrderRequestPayPal>> GetAllOrder();
        Task<Order> GetIdAsync(int OrderID);
        Task<OrderResponsePayPal> Create(OrderRequest order, List<CameraResponse> camera, string userID, decimal? delivery = null, decimal? coupon = null);
        Task<bool> Update(Order order);
        Task<bool> DeleteAsync(int OrderID);
    }
}
