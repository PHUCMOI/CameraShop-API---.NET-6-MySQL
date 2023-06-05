using CameraAPI.Repositories;
using CameraAPI.Services.Interfaces;
using CameraCore.IRepository;
using CameraRepository.Repositories;
using CameraService.Services.IRepositoryServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CameraService.Services.IServices;
using CameraAPI.Services;

namespace CameraService.Services
{
    public static class ServiceExtension
    {
        public static IServiceCollection AddDIServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddScoped<ICameraRepository, CamerasRepository>();
            services.AddScoped<ICameraService, CamerasService>();

            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<ICategoryService, CategoryService>();

            services.AddScoped<IOrderDetailService, OrderDetailService>();
            services.AddScoped<IOrderDetailsRepository, OrderDetailRepository>();

            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserService, UserService>();

            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IOrderRepository, OrderRepository>();

            services.AddScoped<ILoginRepository, LoginRepository>();
            services.AddScoped<ILoginService, LoginService>();

            services.AddScoped<IWarehouseCameraService, WarehouseCameraService>();
            services.AddScoped<IWarehouseCameraRepository, WarehouseCameraRepository>();

            services.AddScoped<IWarehouseCategoryService, WarehouseCategoryService>();
            services.AddScoped<IWarehouseCategoryRepository, WarehouseCategoryRepository>();

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
