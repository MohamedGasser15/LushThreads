using AutoMapper;
using LushThreads.Application.DTOs.User;
using LushThreads.Domain.Entites;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;

namespace LushThreads.Application.Mapping
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            // ApplicationUser -> UserDto
            CreateMap<ApplicationUser, UserDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role ?? "None")) // Role may be populated separately
                .ForMember(dest => dest.IsLocked, opt => opt.MapFrom(src => src.LockoutEnd != null && src.LockoutEnd > DateTime.UtcNow))
                .ForMember(dest => dest.LockoutEnd, opt => opt.MapFrom(src => src.LockoutEnd.HasValue ? src.LockoutEnd.Value.DateTime : (DateTime?)null)) // Fix DateTimeOffset -> DateTime
                .ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.Country))
                .ForMember(dest => dest.PostalCode, opt => opt.MapFrom(src => src.PostalCode))
                .ForMember(dest => dest.StreetAddress, opt => opt.MapFrom(src => src.StreetAddress))
                .ForMember(dest => dest.StreetAddress2, opt => opt.MapFrom(src => src.StreetAddress2))
                .ForMember(dest => dest.PreferredLanguage, opt => opt.MapFrom(src => src.PreferredLanguage))
                .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.Currency))
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.CreatedDate));

            // ApplicationUser -> UserDetailDto
            CreateMap<ApplicationUser, UserDetailDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
                .ForMember(dest => dest.PostalCode, opt => opt.MapFrom(src => src.PostalCode))
                .ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.Country))
                .ForMember(dest => dest.StreetAddress, opt => opt.MapFrom(src => src.StreetAddress))
                .ForMember(dest => dest.StreetAddress2, opt => opt.MapFrom(src => src.StreetAddress2))
                .ForMember(dest => dest.SelectedAddress, opt => opt.MapFrom(src => src.SelectedAddress))
                .ForMember(dest => dest.PreferredLanguage, opt => opt.MapFrom(src => src.PreferredLanguage))
                .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.Currency))
                .ForMember(dest => dest.PaymentMehtod, opt => opt.MapFrom(src => src.PaymentMehtod))
                .ForMember(dest => dest.PreferredCarriers, opt => opt.MapFrom(src => src.PreferredCarriers))
                .ForMember(dest => dest.StripeCustomerId, opt => opt.MapFrom(src => src.StripeCustomerId))
                .ForMember(dest => dest.RoleId, opt => opt.MapFrom(src => src.RoleId))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role))
                .ForMember(dest => dest.IsLocked, opt => opt.MapFrom(src => src.LockoutEnd != null && src.LockoutEnd > DateTime.UtcNow))
                .ForMember(dest => dest.LockoutEnd, opt => opt.MapFrom(src => src.LockoutEnd.HasValue ? src.LockoutEnd.Value.DateTime : (DateTime?)null)) // Fix DateTimeOffset -> DateTime
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.CreatedDate))
                .ForMember(dest => dest.RoleList, opt => opt.Ignore()); // Will be populated separately

            // UpdateUserDto -> ApplicationUser
            CreateMap<UpdateUserDto, ApplicationUser>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
                .ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.Country))
                .ForMember(dest => dest.PostalCode, opt => opt.MapFrom(src => src.PostalCode))
                .ForMember(dest => dest.StreetAddress, opt => opt.MapFrom(src => src.StreetAddress))
                .ForMember(dest => dest.StreetAddress2, opt => opt.MapFrom(src => src.StreetAddress2))
                .ForMember(dest => dest.PreferredLanguage, opt => opt.MapFrom(src => src.PreferredLanguage))
                .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.Currency))
                .ForMember(dest => dest.PaymentMehtod, opt => opt.MapFrom(src => src.PaymentMehtod))
                .ForMember(dest => dest.PreferredCarriers, opt => opt.MapFrom(src => src.PreferredCarriers))
                .ForMember(dest => dest.RoleId, opt => opt.MapFrom(src => src.RoleId))
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null)); // Only map non-null values
        }
    }
}