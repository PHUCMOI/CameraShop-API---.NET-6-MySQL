using CameraAPI.Models;
using CameraAPI.Repositories;
using CameraAPI.Services.Interfaces;
using CameraCore.IRepository;
using CameraRepository.Repositories;
using CameraService.Services;
using CameraService.Services.IRepositoryServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;

namespace CameraAPI.Services
{
    public static class ServiceExtension
    {
        public static IServiceCollection AddDIServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddScoped<ICameraRepository, CamerasRepository>();
            services.AddScoped<ICameraService, CameraService>();

            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<ICategoryService, CategoryService>();

            services.AddScoped<IOrderDetailService, OrderDetailService>();
            services.AddScoped<IOrderDetailsRepository, OrderDetailRepository>();

            return services;
        }
    }
}
