using AutoMapper;
using LushThreads.Application.DTOs.Order;
using LushThreads.Domain.Entites;
using LushThreads.Domain.ViewModels.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LushThreads.Application.Mapping
{
    public class OrderProfile : Profile
    {
        public OrderProfile()
        {
            CreateMap<OrderHeader, OrderHeaderDto>()
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.ApplicationUser != null ? src.ApplicationUser.Name : ""))
                .ForMember(dest => dest.CustomerEmail, opt => opt.MapFrom(src => src.ApplicationUser != null ? src.ApplicationUser.Email : ""));

            CreateMap<OrderDetail, OrderDetailDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Product_Name : ""))
                .ForMember(dest => dest.ProductImage, opt => opt.MapFrom(src => src.Product != null ? src.Product.imgUrl : ""));

            CreateMap<OrderVM, OrderDetailsResponseDto>()
                .ForMember(dest => dest.OrderHeader, opt => opt.MapFrom(src => src.OrderHeader))
                .ForMember(dest => dest.OrderDetails, opt => opt.MapFrom(src => src.OrderDetails));
        }
    }
}
