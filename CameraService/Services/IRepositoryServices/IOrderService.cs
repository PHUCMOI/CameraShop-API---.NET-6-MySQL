using CameraAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CameraService.Services.IRepositoryServices
{
    public interface IOrderService 
    { 
        Task<IEnumerable<Order>> GetAllOrder();
        Task<Order> GetIdAsync(int OrderID);
        Task<bool> Create(Order order);
        Task<bool> Update(Order order);
        Task<bool> DeleteAsync(int OrderID);
    }
}
