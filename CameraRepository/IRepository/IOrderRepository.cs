using CameraAPI.AppModel;
using CameraAPI.Models;

namespace CameraAPI.Repositories
{
    public interface IOrderRepository : IGenericRepository<Order>
    {
        Task<List<OrderRequestPayPal>> GetOrderList();

        Task<decimal> CreateNewOrder(OrderRequest orderRequest, List<CameraResponse> camera, string userID, decimal orderPrice);
    }
}
