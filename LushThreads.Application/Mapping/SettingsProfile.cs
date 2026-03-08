using AutoMapper;
using LushThreads.Application.DTOs.Settings;
using LushThreads.Domain.Entites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LushThreads.Application.Mapping
{
    public class SettingsProfile : Profile
    {
        public SettingsProfile()
        {
            CreateMap<ApplicationUser, UserSettingsDto>()
                .ForMember(dest => dest.PaymentMethod, opt => opt.MapFrom(src => src.PaymentMehtod)); 
        }
    }
}
