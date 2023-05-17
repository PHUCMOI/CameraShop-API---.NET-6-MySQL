using CameraAPI.AppModel;
using CameraAPI.Models;
using CameraAPI.Repositories;
using CameraAPI.Services.Interfaces;
using CameraService.Services.IRepositoryServices;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Data.SqlClient;
using System.Net.Http;
using System.Security.Claims;

namespace CameraAPI.Services
{
    public class CameraService : ICameraService
    {
        public IUnitOfWork _unitOfWork;
        private readonly ICameraRepository _cameraRepository;

        private readonly ICategoryService _categoryService;
        public readonly IWarehouseCameraService _warehouseCameraService;
        public readonly IWarehouseCategoryService _warehouseCategoryService;

        public CameraService(IUnitOfWork unitOfWork, ICategoryService categoryService, IWarehouseCameraService warehouseCameraService,
            IWarehouseCategoryService warehouseCategoryService, ICameraRepository cameraRepository) 
        {
            _unitOfWork = unitOfWork;
            _categoryService = categoryService;
            _warehouseCameraService = warehouseCameraService;
            _warehouseCategoryService = warehouseCategoryService;
            _cameraRepository = cameraRepository;
        }

        public async Task<bool> Create(Camera camera)
        {
            if(camera != null)
            {
                await _unitOfWork.Cameras.Create(camera);

                //Lưu xuống db 
                var result = _unitOfWork.Save();

                if (result > 0)
                    return true;
                else
                    return true;
            }
            return false;
        }    

        public async Task<bool> DeleteAsync(int CameraID)
        {
            if(CameraID > 0)
            {
                var Camera = await _unitOfWork.Cameras.GetById(CameraID);
                if (Camera != null)
                {
                    _unitOfWork.Cameras.Delete(Camera);
                    var result = _unitOfWork.Save();
                    if (result > 0) return true;
                }
            }
            return false;
        }

        public async Task<List<Camera>> GetAllCamera()
        {
            var CameraList = await _unitOfWork.Cameras.GetAll();
            return CameraList;
        }

        public async Task<List<PaginationCameraResponse>> GetCameraByLINQ(int pageNumber, int? categoryID = null,
            string? name = null, string? brand = null, decimal? minPrice = null, decimal? maxPrice = null,
        string? FilterType = null, int? quantity = null)
        {
            try
            {

                var cameras = await GetAllCamera();
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
                                    Description = camera.Description
                                };

                var result = shopQuery;

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
                                         Description = camera.Description
                                     };

                result = result.Union(warehouseQuery);


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
                        result = await CheckFilterTypeAsync(result, FilterType, price);
                    }
                }

                if (quantity.HasValue)
                {
                    result = result.Where(p => p.Quantity == quantity.Value);
                }

                var products = result.Select(camera =>
                {
                    var category = result.FirstOrDefault(x => x.CameraId == camera.CameraId);
                    if (category != null)
                    {
                        return new CameraResponse
                        {
                            CameraName = camera.CameraName,
                            Brand = camera.Brand,
                            Price = camera.Price,
                            Img = camera.Img,
                            Quantity = camera.Quantity,
                            CategoryName = category.CategoryName,
                            Description = camera.Description,
                            BestSeller = "Đã bán " + camera.Sold
                        };
                    }
                    return null;
                })
                .Where(camera => camera != null)
                 .ToList();

                return MapCameraResponse(products, pageNumber);
            }
            catch (Exception ex)
            {
                //_logger.LogInformation(ex.ToString());
            }
            return null;
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

        public async Task<Camera> GetIdAsync(int cameraId)
        {
            if( cameraId > 0 )
            {
                var Camera = await _unitOfWork.Cameras.GetById(cameraId);
                if(Camera != null)
                {
                    return Camera;
                }    
            }
            return null;
        }

        public async Task<bool> Update(Camera camera)
        {
            if(camera != null)
            {
                var cameraDetail = await _unitOfWork.Cameras.GetById(camera.CameraId);
                if(cameraDetail != null)
                {
                    cameraDetail.Name = camera.Name;
                    cameraDetail.Description = camera.Description;
                    cameraDetail.IsDelete = camera.IsDelete;
                    cameraDetail.UpdatedDate = camera.UpdatedDate;
                    cameraDetail.CreatedDate = camera.CreatedDate;
                    cameraDetail.CreatedBy = camera.CreatedBy;
                    cameraDetail.UpdatedBy = camera.UpdatedBy;
                    cameraDetail.Brand = camera.Brand;
                    cameraDetail.CategoryId = camera.CategoryId;
                    cameraDetail.Img = camera.Img;
                    cameraDetail.Price = camera.Price;
                    cameraDetail.Quantity = camera.Quantity;

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

        private List<PaginationCameraResponse> MapCameraResponse(IEnumerable<CameraResponse> cameras, int pageNumber)
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
               // _logger.LogInformation(ex.ToString());
            }
            return null;
        }

        public async Task<List<PaginationCameraResponse>> GetFromStoredProcedure(int pageNumber, int? categoryID = null, string? name = null,
        string? brand = null, decimal? minPrice = null, decimal? maxPrice = null, int? quantity = null)
        {
            try
            {
                var cameras = await _cameraRepository.GetByStoredProcedure(pageNumber, categoryID, name, brand, minPrice, maxPrice, quantity);

                return MapCameraResponse(cameras, pageNumber);
            }
            catch (Exception ex)
            {
                // _logger.LogInformation(ex.ToString());
            }
            return null;
        }
    }
}
