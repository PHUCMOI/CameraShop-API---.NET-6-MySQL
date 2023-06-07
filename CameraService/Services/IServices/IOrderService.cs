using CameraAPI.AppModel;
using CameraAPI.Models;

namespace CameraService.Services.IRepositoryServices
{
    public interface IOrderService 
    {
        Task<IEnumerable<OrderRequestPayPal>> GetAllOrder();
        Task<OrderRequestPayPal> GetIdAsync(int orderID);
        Task<OrderResponsePayPal> Create(OrderRequest order, List<CameraResponse> camera, string userID, decimal? delivery = null, decimal? coupon = null);
        Task<bool> Update(OrderRequest order, string userId, int orderId);
        Task<bool> DeleteAsync(int orderID);
    }
}
