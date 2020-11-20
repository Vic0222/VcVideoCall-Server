using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using Dom = Vc.Domain.Entities;
using Dal = Vc.DAL.Mongo.Collections;

namespace Vc.DAL.Mongo.MapperProfile
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<Dal.User, Dom.User>().ReverseMap();

            CreateMap<Dom.User, Dal.RoomUser>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Nickname, opt => opt.MapFrom(src => src.Username));
        }
    }
}
