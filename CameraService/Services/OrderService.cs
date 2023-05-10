using CameraAPI.Models;
using CameraAPI.Repositories;
using CameraCore.IRepository;
using CameraService.Services.IRepositoryServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CameraService.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        public OrderService(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }
        public Task<bool> Create(Order order)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteAsync(int OrderID)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<Order>> GetAllOrder()
        {
            var OrderlList = await _orderRepository.GetAll();
            return OrderlList;
        }

        public Task<Order> GetIdAsync(int OrderID)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Update(Order order)
        {
            throw new NotImplementedException();
        }
    }
}
