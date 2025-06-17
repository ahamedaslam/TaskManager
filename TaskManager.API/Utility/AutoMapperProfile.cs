using AutoMapper;
using Microsoft.Extensions.DependencyModel;
using System.Security.Cryptography;
using TaskManager.DTOs.Auth;
using TaskManager.DTOs.TaskManager;
using TaskManager.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TaskManager.Utility
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // Add your mapping configurations here
            // For example:
            // CreateMap<SourceModel, DestinationModel>();

            // Map RegisterRequestDTO to ApplicationUser:
            // - Set UserName and Email from Username in the DTO
            // - Set TenantId directly from the DTO


            //REGISTER REQUEST DTO TO APPLICATION USER
            CreateMap<RegisterRequestDTO, ApplicationUser>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Username))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Username))
                .ForMember(dest => dest.TenantId, opt => opt.MapFrom(src => src.TenantId));


            //TaskItem to TaskItemDTO
            CreateMap<TaskItemDTO, TaskItem>()
    .ForMember(dest => dest.IsCompleted, opt => opt.MapFrom(src => false));




            // Entity to DTO
            CreateMap<TaskItem, TaskItemDTO>().ReverseMap();


        }
    }
}
