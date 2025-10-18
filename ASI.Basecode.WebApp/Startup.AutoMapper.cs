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
                CreateMap<UserViewModel, User>();
                CreateMap<MenuItemViewModel, MenuItem>();
                CreateMap<MenuItem, MenuItemViewModel>();

                CreateMap<Cart, CartViewModel>().ReverseMap();

                CreateMap<CartItem, CartItemViewModel>()
                    .ForMember(dest => dest.MenuItemName, opt => opt.MapFrom(src => src.MenuItem.Name))
                    .ForMember(dest => dest.MenuItemDescription, opt => opt.MapFrom(src => src.MenuItem.Description))
                    .ForMember(dest => dest.MenuItemCategory, opt => opt.MapFrom(src => src.MenuItem.Category));

                CreateMap<CartItemViewModel, CartItem>()
                    .ForMember(dest => dest.MenuItem, opt => opt.Ignore());
            }
        }
    }
}
