using CameraAPI.Models;
using CameraAPI.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Configuration;

namespace CameraAPI.Services
{
    public static class ServiceExtension
    {
        public static IServiceCollection AddDIServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<CameraAPIdbContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("InternShop"));
            });
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<ICameraRepository, CameraRepository>();

            return services;
        }
    }
}
