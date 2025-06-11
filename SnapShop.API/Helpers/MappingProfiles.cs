using AutoMapper;
using SnapShop.API.Dtos;
using SnapShop.Core.Entities;
using SnapShop.Core.Entities.OrderAggregate;
using System.Runtime.CompilerServices;

namespace SnapShop.API.Helpers
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles() 
        {
            CreateMap<Product, ProductToReturnDto>()
                .ForMember(D => D.productBrand, O => O.MapFrom(S => S.productBrand.Name))
                .ForMember(D => D.productCategory, O => O.MapFrom(S => S.productCategory.Name))
                .ForMember(D => D.PictureUrl, O => O.MapFrom<ProductPictureUrlResolver>());

            CreateMap<AddressDto, Address>()
                .ForMember(D => D.FirstName, O => O.MapFrom(S => S.FirstName))
                .ForMember(D => D.LastName, O => O.MapFrom(S => S.LastName))
                .ForMember(D => D.Street, O => O.MapFrom(S => S.Street))
                .ForMember(D => D.City, O => O.MapFrom(S => S.City))
                .ForMember(D => D.Country, O => O.MapFrom(S => S.Country))
                .ReverseMap();

            CreateMap<CustomerBasketDto, CustomerBasket>();
            CreateMap<BasketItemDto, BasketItem>();

            CreateMap<Order, OrderToReturnDto>()
                .ForMember(d => d.DeliveryMethod, O => O.MapFrom(s => s.DeliveryMethod.ShortName))
                .ForMember(d => d.DeliveryMethodCost, O => O.MapFrom(s => s.DeliveryMethod.Cost));

            CreateMap<OrderItem, OrderItemDto>()
                .ForMember(d => d.ProductName, O => O.MapFrom(s => s.Product.ProductName))
                .ForMember(d => d.ProductId, O => O.MapFrom(s => s.Product.ProductId))
                .ForMember(d => d.PictureUrl, O => O.MapFrom(s => s.Product.PictureUrl));

        }
    }
}
