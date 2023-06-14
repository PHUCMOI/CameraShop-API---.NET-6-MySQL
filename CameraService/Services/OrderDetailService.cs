using CameraAPI.Models;
using CameraAPI.Repositories;
using CameraAPI.Services.Interfaces;
using CameraCore.IRepository;
using CameraService.Services.IRepositoryServices;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CameraService.Services
{
    public class OrderDetailService : IOrderDetailService
    {
        private readonly IOrderDetailsRepository _orderDetailsRepository;
        public OrderDetailService(IOrderDetailsRepository orderDetailsRepository)
        {
            _orderDetailsRepository = orderDetailsRepository;
        }

        public Task<bool> Create(OrderDetail orderDetail)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteAsync(int OrderID)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<OrderDetail>> GetAllOrderDetail()
        {
            var OrderDetailList = await _orderDetailsRepository.GetAll();
            return OrderDetailList;
        }

        public async Task<OrderDetail> GetIdAsync(int cameraId)
        {
            if (cameraId > 0)
            {
                var orderDetail = await _orderDetailsRepository.GetById(cameraId);
                {
                    return orderDetail;
                }
            }
            throw new Exception();
        }

        public Task<bool> Update(OrderDetail orderDetail)
        {
            throw new NotImplementedException();
        }
    }
}
