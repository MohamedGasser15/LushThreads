using AutoMapper;
using LushThreads.Application.DTOs.AdminActivity;
using LushThreads.Domain.Entites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LushThreads.Application.Mapping
{
    public class AdminActivityProfile : Profile
    {
        public AdminActivityProfile()
        {
            CreateMap<AdminActivity, AdminActivityDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.Name : ""));
        }
    }
}
