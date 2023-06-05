using CameraAPI.Repositories;
using CameraAPI.Services.Interfaces;
using CameraCore.IRepository;
using CameraRepository.Repositories;
using CameraService.Services;
using CameraService.Services.IRepositoryServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CameraService.Services.IServices;

namespace CameraAPI.Services
{
    public static class ServiceExtension
    {
        public static IServiceCollection AddDIServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddTransient<ICameraRepository, CamerasRepository>();
            services.AddTransient<ICameraService, CameraService>();

            services.AddTransient<ICategoryRepository, CategoryRepository>();
            services.AddTransient<ICategoryService, CategoryService>();

            services.AddTransient<IOrderDetailService, OrderDetailService>();
            services.AddTransient<IOrderDetailsRepository, OrderDetailRepository>();

            services.AddTransient<IOrderService, OrderService>();
            services.AddTransient<IOrderRepository, OrderRepository>();

            services.AddTransient<ILoginRepository, LoginRepository>();
            services.AddTransient<ILoginService, LoginService>();

            services.AddTransient<IWarehouseCameraService, WarehouseCameraService>();
            services.AddTransient<IWarehouseCameraRepository, WarehouseCameraRepository>();

            services.AddTransient<IWarehouseCategoryService, WarehouseCategoryService>();
            services.AddTransient<IWarehouseCategoryRepository, WarehouseCategoryRepository>();

            services.AddSingleton<IRedisCacheService, RedisCacheService>();
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = "localhost:6379";
            });

            services.AddTransient<IPayPalService, PayPalService>();

            services.AddLogging();
            services.AddSingleton<IAutoMapperService, AutoMapperService>();

            return services;
        }
    }
}
