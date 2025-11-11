using AutoMapper;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.ServiceModels;
using Microsoft.Extensions.DependencyInjection;

namespace ASI.Basecode.WebApp
{
    // AutoMapper configuration
    internal partial class StartupConfigurer
    {
        /// <summary>
        /// Configure auto mapper
        /// </summary>
        private void ConfigureAutoMapper()
        {
            var mapperConfiguration = new MapperConfiguration(config =>
            {
                config.AddProfile(new AutoMapperProfileConfiguration());
            });

            this._services.AddSingleton<IMapper>(sp => mapperConfiguration.CreateMapper());
        }

        private class AutoMapperProfileConfiguration : Profile
        {
            public AutoMapperProfileConfiguration()
            {
                // User mappings
                CreateMap<User, UserViewModel>().ReverseMap();
                CreateMap<User, ProfileViewModel>().ReverseMap();

                // MenuItem mappings
                CreateMap<MenuItemViewModel, MenuItem>();
                CreateMap<MenuItem, MenuItemViewModel>();

                // Cart mappings
                CreateMap<Cart, CartViewModel>().ReverseMap();
                CreateMap<CartItem, CartItemViewModel>()
                    .ForMember(dest => dest.MenuItemName, opt => opt.MapFrom(src => src.MenuItem.Name))
                    .ForMember(dest => dest.MenuItemDescription, opt => opt.MapFrom(src => src.MenuItem.Description))
                    .ForMember(dest => dest.MenuItemCategory, opt => opt.MapFrom(src => src.MenuItem.Category))
                    .ForMember(dest => dest.MenuItemImagePath, opt => opt.MapFrom(src => src.MenuItem.ImagePath));
                CreateMap<CartItemViewModel, CartItem>()
                    .ForMember(dest => dest.MenuItem, opt => opt.Ignore());

                // Order mappings
                CreateMap<Order, OrderViewModel>()
                    .ForMember(dest => dest.OrderItems, opt => opt.MapFrom(src => src.OrderItems))
                    .ReverseMap();
                CreateMap<OrderItem, OrderItemViewModel>().ReverseMap();
            }
        }
    }
}
