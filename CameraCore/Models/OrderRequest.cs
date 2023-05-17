using CameraAPI.Models;

namespace CameraAPI.AppModel
{
    public class OrderRequest
    {
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Address { get; set; }
        public string Payment { get; set; }
        public string Status { get; set; }
        public decimal Price { get; set; }
        public string Message { get; set; }
        public List<OrderDetail1> OrderDetails { get; set; }
    }

    public class OrderDetail1
    {
        public int? OrderId { get; set; }
        public int? CameraId { get; set; }
        public decimal? Quantity { get; set; }
        public string Status { get; set; }
        public Camera1 Camera { get; set; }
    }

    public class Camera1
    {
        public int CameraId { get; set; }
        public int? CategoryId { get; set; }
        public string Name { get; set; }
        public string Brand { get; set; }
        public string Description { get; set; }
        public decimal? Price { get; set; }
        public string Img { get; set; }
        public int? Quantity { get; set; }
        public int Sold { get; set; }

    }
}
