using CameraAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CameraAPI.Services.Interfaces
{
    public interface IOrderDetailService
    {
        Task<IEnumerable<OrderDetail>> GetAllOrderDetail();
        Task<OrderDetail> GetIdAsync(int CameraID);
        Task<bool> Create(OrderDetail orderDetail);
        Task<bool> Update(OrderDetail orderDetail);
        Task<bool> DeleteAsync(int OrderID);
    }
}
