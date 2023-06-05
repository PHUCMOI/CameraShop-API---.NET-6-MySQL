using AutoMapper;
using CameraAPI.AppModel;
using CameraAPI.Models;
using CameraCore.Models;
using CameraService.Services.IRepositoryServices;
using CameraService.Services.IServices;
using System.Collections.Generic;

namespace CameraService.Services
{
    public class AutoMapperService : IAutoMapperService
    {
        private readonly IMapper _mapper;

        public AutoMapperService()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new CategoryMappingProfile());
                cfg.AddProfile(new CameraMappingProfile());
                cfg.AddProfile(new CameraIDMappingProfile());
                cfg.AddProfile(new OrderMappingProfile());
                cfg.AddProfile(new UserMappingProfile());
            });

            _mapper = config.CreateMapper();
        }

        public TDestination Map<TSource, TDestination>(TSource source)
        {
            return _mapper.Map<TSource, TDestination>(source);
        }

        public IEnumerable<TDestination> MapList<TSource, TDestination>(IEnumerable<TSource> sourceList)
        {
            return _mapper.Map<IEnumerable<TSource>, IEnumerable<TDestination>>(sourceList);
        }
        public class CategoryMappingProfile : Profile
        {
            public CategoryMappingProfile()
            {
                CreateMap<Category, CategoryResponse>();
            }
        }

        public class CameraMappingProfile : Profile
        { 
            public CameraMappingProfile()
            {
                CreateMap<Camera, CameraResponse>()
                    .ForMember(dest => dest.CameraName, opt => opt.MapFrom(src => src.Name))
                    .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.CategoryId))
                    .ForMember(dest => dest.BestSeller, opt => opt.MapFrom(src => src.Sold));
            }
        }
        public class CameraIDMappingProfile : Profile
        {
            public CameraIDMappingProfile()
            {
                CreateMap<Camera, CameraResponseID>()
                    .ForMember(dest => dest.CameraName, opt => opt.MapFrom(src => src.Name))
                    .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.CategoryId));
            }
        }

        public class OrderMappingProfile : Profile
        {
            public OrderMappingProfile()
            {
                CreateMap<Order, OrderResponse>();
            }
        }

        public class UserMappingProfile : Profile
        {
            public UserMappingProfile()
            {
                CreateMap<User, UserResponse>();
            }
        }
    }
}
