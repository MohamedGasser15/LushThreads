using AutoMapper;
using LushThreads.Application.DTOs.Category;
using LushThreads.Domain.Entites;

namespace LushThreads.Application.Mapping
{
    public class CategoryProfile : Profile
    {
        public CategoryProfile()
        {
            // Entity -> DTO
            CreateMap<Category, CategoryDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Category_Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Category_Name));

            // Create DTO -> Entity
            CreateMap<CreateCategoryDto, Category>()
                .ForMember(dest => dest.Category_Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Category_Id, opt => opt.Ignore());

            // Update DTO -> Entity
            CreateMap<UpdateCategoryDto, Category>()
                .ForMember(dest => dest.Category_Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Category_Name, opt => opt.MapFrom(src => src.Name));
        }
    }
}