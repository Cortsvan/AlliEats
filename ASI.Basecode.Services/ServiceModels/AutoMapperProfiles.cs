using ASI.Basecode.Data.Models;
using AutoMapper;

namespace ASI.Basecode.Services.ServiceModels
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            // User mappings
            CreateMap<User, UserViewModel>().ReverseMap();

            // MenuItem mappings
            CreateMap<MenuItem, MenuItemViewModel>().ReverseMap();

            // Cart mappings
            CreateMap<Cart, CartViewModel>().ReverseMap();
            CreateMap<CartItem, CartItemViewModel>()
                .ForMember(dest => dest.MenuItemName, opt => opt.MapFrom(src => src.MenuItem.Name))
                .ForMember(dest => dest.MenuItemDescription, opt => opt.MapFrom(src => src.MenuItem.Description))
                .ForMember(dest => dest.MenuItemCategory, opt => opt.MapFrom(src => src.MenuItem.Category))
                .ReverseMap();

            // Order mappings
            CreateMap<Order, OrderViewModel>()
                .ForMember(dest => dest.OrderItems, opt => opt.MapFrom(src => src.OrderItems))
                .ReverseMap();

            CreateMap<OrderItem, OrderItemViewModel>().ReverseMap();
        }
    }
}