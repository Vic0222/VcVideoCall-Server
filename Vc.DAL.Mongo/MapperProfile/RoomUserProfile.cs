using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using Dom = Vc.Domain.Entities;
using Dal = Vc.DAL.Mongo.Collections;

namespace Vc.DAL.Mongo.MapperProfile
{
    public class RoomUserProfile : Profile
    {
        public RoomUserProfile()
        {
            CreateMap<Dal.RoomUser, Dom.RoomUser>()
                .ForMember(dest => dest.Status, cfg => cfg.MapFrom(src => src.Status))
                .ReverseMap();
        }
    }
}
