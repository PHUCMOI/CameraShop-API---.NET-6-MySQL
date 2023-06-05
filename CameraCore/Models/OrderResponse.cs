namespace CameraAPI.AppModel
{
    public class OrderResponsePayPal
    {
        public int requestID { get; set; }
        public int orderID { get; set; }
        public string price { get; set; }
        public DateTime responseTime { get; set; }    
        public string orderStatus { get; set; }
        public string statusCode { get; set; }
        public string errorCode { get; set; }
        public string payUrl { get; set; }
    }

    public class OrderResponse
    {
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Address { get; set; }
        public string Payment { get; set; }
        public string Status { get; set; }
        public decimal? Price { get; set; }
        public string Message { get; set; }
    }
}
