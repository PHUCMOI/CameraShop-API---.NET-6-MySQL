namespace CameraAPI.AppModel
{
    public class PaginationCameraResponse
    {
        public List<CameraResponse> Camera { get; set; } = new List<CameraResponse>();
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int TotalPage { get; set; }
    }
}
