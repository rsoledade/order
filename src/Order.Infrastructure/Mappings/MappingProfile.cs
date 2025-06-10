using AutoMapper;
using Order.Application.DTOs;
using Order.Application.Events;
using Order.Domain.Entities;

namespace Order.Infrastructure.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Entity to DTO mappings
            CreateMap<Order.Domain.Entities.Order, OrderDto>()
                .ForMember(dest => dest.OrderId, opt => opt.MapFrom(src => src.OrderId.Value))
                .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => src.TotalAmount.Value));

            CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price.Value));

            // DTO to Entity mappings
            CreateMap<ProductDto, Product>()
                .ForCtorParam("name", opt => opt.MapFrom(src => src.Name))
                .ForCtorParam("price", opt => opt.MapFrom(src => Domain.ValueObjects.Money.Create(src.Price)))
                .ForCtorParam("quantity", opt => opt.MapFrom(src => src.Quantity));

            // Event mappings
            CreateMap<Product, OrderProductEvent>()
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price.Value));
        }
    }
}
