using AutoMapper;
using LushThreads.Application.DTOs.Auth;
using LushThreads.Domain.Entites;
using LushThreads.Domain.ViewModels.Auth;

namespace LushThreads.Application.Mapping
{
    public class AuthProfile : Profile
    {
        public AuthProfile()
        {
            CreateMap<RegisterRequestDto, RegisterViewModel>();
            CreateMap<VerifyEmailRequestDto, VerifyEmailViewModel>();
            CreateMap<ResendVerificationRequestDto, ResendVerificationRequestDto>();
            CreateMap<ForgotPasswordRequestDto, ForgotPasswordViewModel>();
            CreateMap<VerifyResetCodeRequestDto, VerifyCodeViewModel>();
            CreateMap<ResetPasswordRequestDto, ResetPasswordViewModel>();
            CreateMap<TwoFactorVerifyDto, TwoFactorSetupViewModel>();
        }
    }
}