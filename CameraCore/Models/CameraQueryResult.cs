namespace CameraAPI.AppModel{
    public class CameraQueryResult
    {
        public int CameraId { get; set; }
        public string CameraName { get; set; }
        public string Brand { get; set; }
        public decimal Price { get; set; }
        public int Sold { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public bool IsWarehouseCamera { get; set; }
        public int Quantity { get; set; }
        public string Img { get; set; }
        public string Description { get; set; }
        public int Rank { get; set; }
    }
}
