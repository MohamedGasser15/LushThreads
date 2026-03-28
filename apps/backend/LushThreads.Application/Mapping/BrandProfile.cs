using AutoMapper;
using LushThreads.Application.DTOs.Brand;
using LushThreads.Domain.Entites;

namespace LushThreads.Application.Mapping
{
    public class BrandProfile : Profile
    {
        public BrandProfile()
        {
            // Entity to DTO
            CreateMap<Brand, BrandDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Brand_Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Brand_Name));

            // Create DTO to Entity
            CreateMap<CreateBrandDto, Brand>()
                .ForMember(dest => dest.Brand_Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Brand_Id, opt => opt.Ignore()); // Ignore Id because it's auto-generated

            // Update DTO to Entity
            CreateMap<UpdateBrandDto, Brand>()
                .ForMember(dest => dest.Brand_Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Brand_Name, opt => opt.MapFrom(src => src.Name));
        }
    }
}
