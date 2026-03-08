using AutoMapper;
using LushThreads.Application.DTOs.Product;
using LushThreads.Domain.Entites;
using LushThreads.Domain.ViewModels.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LushThreads.Application.Mapping
{
    public class ProductProfile : Profile
    {
        public ProductProfile()
        {
            // Product → ProductDto
            CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Product_Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Product_Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Product_Description))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Product_Price))
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.imgUrl))
                .ForMember(dest => dest.Rating, opt => opt.MapFrom(src => src.Product_Rating))
                .ForMember(dest => dest.Color, opt => opt.MapFrom(src => src.Product_Color))
                .ForMember(dest => dest.IsFeatured, opt => opt.MapFrom(src => src.IsFeatured))
                .ForMember(dest => dest.DateAdded, opt => opt.MapFrom(src => src.DateAdded))
                .ForMember(dest => dest.IsNewArrival, opt => opt.MapFrom(src => src.IsNewArrival))
                .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.Category_Id))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Category_Name : null))
                .ForMember(dest => dest.BrandId, opt => opt.MapFrom(src => src.brand_Id))
                .ForMember(dest => dest.BrandName, opt => opt.MapFrom(src => src.Brand != null ? src.Brand.Brand_Name : null))
                .ForMember(dest => dest.Stocks, opt => opt.MapFrom(src => src.Stocks));

            // Stock → StockDto
            CreateMap<Stock, StockDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Stock_Id))
                .ForMember(dest => dest.Size, opt => opt.MapFrom(src => src.Size))
                .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity))
                .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.Product_Id));

            // من CreateProductRequestDto إلى ProductViewModel (جزئي)
            CreateMap<CreateProductRequestDto, ProductViewModel>()
                .ForMember(dest => dest.Product, opt => opt.MapFrom(src => new Product
                {
                    Product_Name = src.Name,
                    Product_Description = src.Description,
                    Product_Price = src.Price,
                    Product_Color = src.Color,
                    Product_Rating = src.Rating,
                    IsFeatured = src.IsFeatured,
                    Category_Id = src.CategoryId,
                    brand_Id = src.BrandId
                }))
                .ForMember(dest => dest.Stocks, opt => opt.MapFrom(src => src.Stocks.Select(s => new Stock
                {
                    Size = s.Size,
                    Quantity = s.Quantity
                }).ToList()));

            // من UpdateProductRequestDto إلى ProductViewModel
            CreateMap<UpdateProductRequestDto, ProductViewModel>()
                .ForMember(dest => dest.Product, opt => opt.MapFrom(src => new Product
                {
                    Product_Id = src.Id,
                    Product_Name = src.Name,
                    Product_Description = src.Description,
                    Product_Price = src.Price,
                    Product_Color = src.Color,
                    Product_Rating = src.Rating,
                    IsFeatured = src.IsFeatured,
                    Category_Id = src.CategoryId,
                    brand_Id = src.BrandId
                }))
                .ForMember(dest => dest.Stocks, opt => opt.MapFrom(src => src.Stocks.Select(s => new Stock
                {
                    Size = s.Size,
                    Quantity = s.Quantity
                }).ToList()));
        }
    }
}
