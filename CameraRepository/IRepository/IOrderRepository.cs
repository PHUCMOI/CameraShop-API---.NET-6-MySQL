using CameraAPI.AppModel;
using CameraAPI.Models;

namespace CameraAPI.Repositories
{
    public interface IOrderRepository : IGenericRepository<Order>
    {
        Task<List<OrderRequestPayPal>> GetOrderList();
        Task<OrderRequestPayPal> GetOrderById(int orderId);
        Task<decimal> CreateNewOrder(OrderRequest orderRequest, List<CameraResponse> camera, string userID, decimal orderPrice);
        bool Delete(int orderId);
        void Update(OrderRequest order, string userId, int orderId);
    }
}
