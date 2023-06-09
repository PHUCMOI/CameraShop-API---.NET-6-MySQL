using CameraAPI.AppModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CameraCore.Models
{
    public class PaginationOrderResponse
    {
        public List<OrderRequestPayPal> Orders { get; set; } = new List<OrderRequestPayPal>();
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int TotalPage { get; set; }
    }
}
