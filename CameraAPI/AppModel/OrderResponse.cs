namespace CameraAPI.AppModel
{
    public class OrderResponse
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
}
