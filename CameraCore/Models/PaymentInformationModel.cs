using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CameraCore.Models
{
    public class PaymentInformationModel
    {
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Address { get; set; }
        public string Payment { get; set; }
        public string Status { get; set; }
        public decimal Price { get; set; }
        public string Message { get; set; }
        public List<OrderDetail2> OrderDetails { get; set; }
    }

    public class OrderDetail2
    {
        public int? OrderId { get; set; }
        public int? CameraId { get; set; }
        public decimal? Quantity { get; set; }
        public string Status { get; set; }
        public Camera2 Camera { get; set; }
    }

    public class Camera2
    {
        public int? CategoryId { get; set; }
        public string Name { get; set; }
        public string Brand { get; set; }
        public string Description { get; set; }
        public decimal? Price { get; set; }
        public string Img { get; set; }

    }
}
