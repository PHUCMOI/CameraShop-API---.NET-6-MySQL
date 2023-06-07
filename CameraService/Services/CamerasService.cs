using CameraAPI.AppModel;
using CameraAPI.Models;
using CameraAPI.Repositories;
using CameraAPI.Services.Interfaces;
using CameraCore.Models;
using CameraService.Services.IRepositoryServices;
using CameraService.Services.IServices;
using Castle.Core.Logging;
using Microsoft.Extensions.Logging;
using PayPal.v1.Orders;

namespace CameraAPI.Services
{
    public class CamerasService : ICameraService
    {
        private IUnitOfWork _unitOfWork;
        private readonly ICameraRepository _cameraRepository;

        private readonly ICategoryService _categoryService;
        private readonly IWarehouseCameraService _warehouseCameraService;
        private readonly IWarehouseCategoryService _warehouseCategoryService;

        private readonly IAutoMapperService _autoMapperService;

        private ILogger<CamerasService> _logger;

        public CamerasService(IUnitOfWork unitOfWork, ICategoryService categoryService, IWarehouseCameraService warehouseCameraService,
            IWarehouseCategoryService warehouseCategoryService, ICameraRepository cameraRepository, ILogger<CamerasService> logger,
            IAutoMapperService autoMapperService) 
        {
            _unitOfWork = unitOfWork;
            _categoryService = categoryService;
            _warehouseCameraService = warehouseCameraService;
            _warehouseCategoryService = warehouseCategoryService;
            _cameraRepository = cameraRepository;
            _logger = logger;
            _autoMapperService = autoMapperService;
        }

        public async Task<bool> Create(CameraPostRequest cameraPostRequest, string UserID)
        {
            if(cameraPostRequest != null)
            {
                var camera = new Camera()
                {
                    Name = cameraPostRequest.Name,
                    CategoryId = cameraPostRequest.CategoryId,
                    Brand = cameraPostRequest.Brand,
                    Description = cameraPostRequest.Description,
                    Price = cameraPostRequest.Price,
                    Img = cameraPostRequest.Img,
                    Quantity = cameraPostRequest.Quantity,
                    CreatedBy = Convert.ToInt16(UserID),
                    CreatedDate = DateTime.Now,
                    UpdatedBy = Convert.ToInt16(UserID),   
                    UpdatedDate = DateTime.Now,
                    IsDelete = false
                };

                await _cameraRepository.Create(camera);

                //Lưu xuống db 
                var result = _unitOfWork.Save();

                if (result > 0)
                    return true;
                else
                    return true;
            }
            return false;
        }    

        public Task<bool> DeleteAsync(int cameraID)
        {
            if(cameraID > 0)
            {
                var camera = _cameraRepository.Delete(cameraID);
                if (camera)
                {
                    var result = _unitOfWork.Save();
                    if (result == 0) return Task.FromResult(true);
                }
            }
            return Task.FromResult(false);
        }

        public async Task<List<CameraResponse>> GetAllCamera()
        {
            var cameraList = await _cameraRepository.GetCameraList();
            var categories = await _categoryService.GetAllCategory();
            var cameraResponseList = _autoMapperService.MapList<Camera, CameraResponse>(cameraList);

            var sortedCameraResponseList = cameraResponseList.OrderByDescending(c => c.BestSeller).ThenByDescending(c => c.CategoryName).ToList();

            int rank = 1;
            int previousCount = 0;

            for (int i = 0; i < sortedCameraResponseList.Count; i++)
            {
                var cameraResponse = sortedCameraResponseList[i];

                var currentCount = Convert.ToInt32(cameraResponse.BestSeller);

                var category = categories.FirstOrDefault(c => c.CategoryId == Convert.ToInt16(cameraResponse.CategoryName));
                if (category != null)
                {
                    cameraResponse.CategoryName = category.Name;
                }

                if (i > 0 && currentCount != previousCount)
                {
                    rank = i + 1;
                }

                cameraResponse.BestSeller = "Top " + rank + " Seller";

                previousCount = currentCount;
            }

            return sortedCameraResponseList;
        }




        public async Task<List<PaginationCameraResponse>> GetCameraByLINQ(int pageNumber, int? categoryID = null,
            string? name = null, string? brand = null, decimal? minPrice = null, decimal? maxPrice = null,
            string? FilterType = null, int? quantity = null)
        {
            try
            {
                var cameras = await _cameraRepository.GetCameraList();
                var categories = await _categoryService.GetAllCategory();

                var shopQuery = from camera in cameras
                                join category in categories
                                on camera.CategoryId equals category.CategoryId into joinedCategories
                                from category in joinedCategories.DefaultIfEmpty()
                                orderby camera.Sold descending
                                select new CameraQueryResult
                                {
                                    CameraId = camera.CameraId,
                                    CameraName = camera.Name,
                                    Brand = camera.Brand,
                                    Price = (decimal)camera.Price,
                                    Sold = camera.Sold,
                                    CategoryId = (int)camera.CategoryId,
                                    CategoryName = category != null ? category.Name : "N/A",
                                    IsWarehouseCamera = false,
                                    Quantity = (int)camera.Quantity,
                                    Img = camera.Img,
                                    Description = camera.Description,
                                };

                var rankedShopQuery = shopQuery.ToList();

                var result = rankedShopQuery.AsQueryable();

                var warehouseCamera = await _warehouseCameraService.GetAllCamera();
                var warehouseCategory = await _warehouseCategoryService.GetAllCategory();
                var warehouseQuery = from camera in warehouseCamera
                                     join category in warehouseCategory
                                     on camera.CategoryId equals category.CategoryId into joinedCategories
                                     from category in joinedCategories.DefaultIfEmpty()
                                     orderby camera.Sold descending
                                     select new CameraQueryResult
                                     {
                                         CameraId = camera.CameraId,
                                         CameraName = camera.Name,
                                         Brand = camera.Brand,
                                         Price = (decimal)camera.Price,
                                         Sold = camera.Sold,
                                         CategoryId = (int)camera.CategoryId,
                                         CategoryName = category != null ? category.Name : "N/A",
                                         IsWarehouseCamera = true,
                                         Quantity = (int)camera.Quantity,
                                         Img = camera.Img,
                                         Description = camera.Description,
                                     };

                var rankedWarehouseQuery = warehouseQuery.ToList();

                // Gán rank cho các mục có số lượng bán hàng bằng nhau
                var groupedResult = result
                    .Concat(rankedWarehouseQuery)                    
                    .ToList();

                int rank = 1;
                int previousCount = 0;

                for (int i = 0; i < groupedResult.Count; i++)
                {
                    var cameraResponse = groupedResult[i];
                    var currentCount = Convert.ToInt32(cameraResponse.Sold);
                   
                    if (i > 0 && currentCount != previousCount)
                    {
                        rank = i + 1;
                    }

                    cameraResponse.Rank = rank;

                    previousCount = currentCount;
                }

                result = groupedResult.AsQueryable();




                if (categoryID.HasValue)
                {
                    result = result.Where(p => p.CategoryId == categoryID.Value);
                }

                if (!string.IsNullOrEmpty(name))
                {
                    result = result.Where(p => p.CameraName.Contains(name));
                }

                if (!string.IsNullOrEmpty(brand))
                {
                    result = result.Where(p => p.Brand.Contains(brand));
                }

                if (minPrice.HasValue && maxPrice.HasValue)
                {
                    result = result.Where(p => p.Price >= minPrice.Value && p.Price <= maxPrice.Value);
                }
                else if (maxPrice.HasValue || minPrice.HasValue)
                {
                    if (!string.IsNullOrEmpty(FilterType) && (maxPrice.HasValue || minPrice.HasValue))
                    {
                        decimal? price = maxPrice.HasValue ? maxPrice : minPrice;
                        result = (IQueryable<CameraQueryResult>)await CheckFilterTypeAsync(result, FilterType, price);
                    }
                }

                if (quantity.HasValue)
                {
                    result = result.Where(p => p.Quantity == quantity.Value);
                }

                var products = result
                    .Select(camera => new
                    {
                        Camera = camera,
                        Category = result.FirstOrDefault(x => x.CameraId == camera.CameraId)
                    })
                    .Where(pair => pair.Category != null)
                    .Select(pair => new CameraResponse
                    {
                        CameraID = pair.Camera.CameraId,
                        CameraName = pair.Camera.CameraName,
                        Brand = pair.Camera.Brand,
                        Price = pair.Camera.Price,
                        Img = pair.Camera.Img,
                        Quantity = pair.Camera.Quantity,
                        CategoryName = pair.Category.CategoryName,
                        Description = pair.Camera.Description,
                        BestSeller = "Top " + pair.Camera.Rank + " Seller"
                    })
                    .ToList();


                return MapCameraResponse(products, pageNumber);
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.ToString());
                return null;
            }
        }

        private async Task<List<CameraQueryResult>> CheckFilterTypeAsync(IEnumerable<CameraQueryResult> products, string filter, decimal? price = null)
        {
            List<CameraQueryResult> filteredProducts = products.ToList();

            switch (filter)
            {
                case "lte":
                    filteredProducts = filteredProducts.Where(p => p.Price <= price).ToList();
                    break;
                case "gte":
                    filteredProducts = filteredProducts.Where(p => p.Price >= price).ToList();
                    break;
                default:
                    break;
            }

            return filteredProducts;
        }

        public async Task<CameraResponseID> GetIdAsync(int cameraId)
        {
            if( cameraId > 0 )
            {
                var camera = await _cameraRepository.GetById(cameraId);
                if (camera.IsDelete == true)
                {
                    return null;
                }
                else
                {

                    var category = await _categoryService.GetIdAsync((int)camera.CategoryId);
                    if (camera != null)
                    {
                        var cameraResponse = _autoMapperService.Map<Camera, CameraResponseID>(camera);
                        cameraResponse.CategoryName = category.Name;
                        return cameraResponse;
                    }
                }
            }
            return null;
        }

        public async Task<bool> Update(CameraPostRequest cameraRequest, string UserID, int id)
        {
            if(cameraRequest != null)
            {
                var cameraDetail = await _unitOfWork.Cameras.GetById(id);
                var category = await _categoryService.GetIdAsync((int)cameraDetail.CategoryId);

                if (cameraDetail != null)
                {
                    cameraDetail.Name = cameraRequest.Name;
                    cameraDetail.Description = cameraRequest.Description;
                    cameraDetail.IsDelete = false;
                    cameraDetail.UpdatedDate = DateTime.Now;
                    cameraDetail.CreatedDate = DateTime.Now;
                    cameraDetail.CreatedBy = Convert.ToInt16(UserID);
                    cameraDetail.UpdatedBy = Convert.ToInt16(UserID);
                    cameraDetail.Brand = cameraRequest.Brand;
                    cameraDetail.CategoryId = category.CategoryId;
                    cameraDetail.Img = cameraRequest.Img;
                    cameraDetail.Price = cameraRequest.Price;
                    cameraDetail.Quantity = cameraRequest.Quantity;

                    _unitOfWork.Cameras.Update(cameraDetail);
                    var result = _unitOfWork.Save();
                    if(result > 0) 
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private List<PaginationCameraResponse> MapCameraResponse(List<CameraResponse> cameras, int pageNumber)
        {
            var cameraList = cameras.ToList();
            var count = cameraList.Count;
            var pageSize = 3;
            var totalPage = (int)Math.Ceiling((decimal)count / pageSize);
            if (pageNumber == 0) pageNumber = 1;

            var paginationResponse = new PaginationCameraResponse
            {
                Camera = cameraList.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList(),
                PageIndex = pageNumber,
                PageSize = pageSize,
                TotalPage = totalPage
            };

            return new List<PaginationCameraResponse> { paginationResponse };
        }

        public async Task<List<PaginationCameraResponse>> GetCameraBySQL(int pageNumber, int? categoryID = null, string? name = null, string? brand = null, decimal? minPrice = null, decimal? maxPrice = null, string? FilterType = null, int? quantity = null)
        {
            try
            {
                var cameras = await _cameraRepository.GetBySQL(pageNumber, categoryID, name, brand, minPrice, maxPrice, FilterType, quantity);
                
                return MapCameraResponse(cameras, pageNumber);                
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.Message, ex);                
                return null;
            }
        }

        public async Task<List<PaginationCameraResponse>> GetFromStoredProcedure(int pageNumber, int? categoryID = null, string? name = null,
        string? brand = null, decimal? minPrice = null, decimal? maxPrice = null, string? FilterType = null, int? quantity = null)
        {
            try
            {
                var cameras = await _cameraRepository.GetByStoredProcedure(pageNumber, categoryID, name, brand, minPrice, maxPrice, FilterType,quantity);

                return MapCameraResponse(cameras, pageNumber);
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.ToString());
                return null;
            }
        }
    }
}
