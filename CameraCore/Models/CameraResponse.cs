namespace CameraAPI.AppModel
{
    public class CameraResponse
    {
        public int CameraID { get; set; }
        public string CameraName { get; set; }
        public string Brand { get; set; }
        public decimal? Price { get; set; }
        public string Img { get; set; }
        public int? Quantity { get; set; }
        public string CategoryName { get; set; }
        public string Description { get; set; }
        public string BestSeller { get; set; }
    }

    public class CameraResponseID
    {
        public int CameraID { get; set; }
        public string CameraName { get; set; }
        public string Brand { get; set; }
        public decimal? Price { get; set; }
        public string Img { get; set; }
        public int? Quantity { get; set; }
        public string CategoryName { get; set; }
        public string Description { get; set; }
    }
}
